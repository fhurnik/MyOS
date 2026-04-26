using System.Data;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

public class SqlViewSynchronizer
{
    // Logger for structured logging of operations and errors
    private readonly ILogger<SqlViewSynchronizer> _logger;
    // Database connection for executing SQL and tracking file history
    private readonly IDbConnection _dbConnection;
    // Root directory where SQL view files are located
    private readonly string _viewsRoot;

    /// <summary>
    /// Initializes a new instance of the SqlViewSynchronizer.
    /// </summary>
    /// <param name="logger">Logger for logging actions and errors.</param>
    /// <param name="dbConnection">Database connection to SQL Server.</param>
    /// <param name="viewsRoot">Root directory for SQL view files.</param>
    public SqlViewSynchronizer(ILogger<SqlViewSynchronizer> logger, IDbConnection dbConnection, string viewsRoot)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbConnection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));
        _viewsRoot = viewsRoot ?? throw new ArgumentNullException(nameof(viewsRoot));
    }

    /// <summary>
    /// Synchronizes all .sql files in the views directory with the database.
    /// - Scans recursively for .sql files.
    /// - Computes SHA256 hash of each file's content.
    /// - Checks if the file and hash exist in system.sql_file_history.
    /// - Executes SQL and updates/inserts history if new or changed.
    /// - Does nothing if hash is unchanged.
    /// </summary>
    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        // Find all .sql files recursively in the views root directory
        var sqlFiles = Directory.GetFiles(_viewsRoot, "*.sql", SearchOption.AllDirectories);

        foreach (var filePath in sqlFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Get the file name relative to the root, using forward slashes
            var fileName = Path.GetRelativePath(_viewsRoot, filePath).Replace('\\', '/');
            // Read the SQL file content
            var sql = await File.ReadAllTextAsync(filePath, cancellationToken);
            // Compute SHA256 hash of the file content
            var hash = ComputeSha256(sql);

            // Check if this file and hash already exist in the tracking table
            var (exists, currentHash) = await GetFileHistoryAsync(fileName, cancellationToken);

            if (!exists)
            {
                // New file: execute SQL and insert history record
                _logger.LogInformation("Executing new SQL view: {FileName}", fileName);
                await ExecuteSqlAsync(sql, cancellationToken);
                await InsertFileHistoryAsync(fileName, hash, cancellationToken);
            }
            else if (!string.Equals(currentHash, hash, StringComparison.OrdinalIgnoreCase))
            {
                // File changed: re-execute SQL and update hash in history
                _logger.LogInformation("Updating SQL view: {FileName}", fileName);
                await ExecuteSqlAsync(sql, cancellationToken);
                await UpdateFileHistoryAsync(fileName, hash, cancellationToken);
            }
            else
            {
                // File unchanged: do nothing
                _logger.LogDebug("No change for SQL view: {FileName}", fileName);
            }
        }
    }

    /// <summary>
    /// Computes the SHA256 hash of the given string content.
    /// </summary>
    private static string ComputeSha256(string content)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = sha.ComputeHash(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Checks if a file is tracked in the sql_file_history table and returns its hash if present.
    /// </summary>
    private async Task<(bool exists, string hash)> GetFileHistoryAsync(string fileName, CancellationToken cancellationToken)
    {
        using var cmd = _dbConnection.CreateCommand();
        cmd.CommandText = "SELECT [hash] FROM [system].[sql_file_history] WHERE [file_name] = @fileName";
        var param = cmd.CreateParameter();
        param.ParameterName = "@fileName";
        param.Value = fileName;
        cmd.Parameters.Add(param);

        if (_dbConnection.State != ConnectionState.Open)
            _dbConnection.Open();

        // Execute the query asynchronously
        using var reader = await Task.Run(() => cmd.ExecuteReader(), cancellationToken);
        if (reader.Read())
        {
            // File exists, return its hash
            return (true, reader.GetString(0));
        }
        // File not found in history
        return (false, null);
    }

    /// <summary>
    /// Inserts a new file record into sql_file_history after executing its SQL.
    /// </summary>
    private async Task InsertFileHistoryAsync(string fileName, string hash, CancellationToken cancellationToken)
    {
        using var cmd = _dbConnection.CreateCommand();
        cmd.CommandText = "INSERT INTO [system].[sql_file_history] ([file_name], [hash], [applied_at_utc]) VALUES (@fileName, @hash, SYSUTCDATETIME())";
        var p1 = cmd.CreateParameter();
        p1.ParameterName = "@fileName";
        p1.Value = fileName;
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.ParameterName = "@hash";
        p2.Value = hash;
        cmd.Parameters.Add(p2);

        await Task.Run(() => cmd.ExecuteNonQuery(), cancellationToken);
    }

    /// <summary>
    /// Updates the hash and applied time for an existing file in sql_file_history after re-executing its SQL.
    /// </summary>
    private async Task UpdateFileHistoryAsync(string fileName, string hash, CancellationToken cancellationToken)
    {
        using var cmd = _dbConnection.CreateCommand();
        cmd.CommandText = "UPDATE [system].[sql_file_history] SET [hash] = @hash, [applied_at_utc] = SYSUTCDATETIME() WHERE [file_name] = @fileName";
        var p1 = cmd.CreateParameter();
        p1.ParameterName = "@hash";
        p1.Value = hash;
        cmd.Parameters.Add(p1);

        var p2 = cmd.CreateParameter();
        p2.ParameterName = "@fileName";
        p2.Value = fileName;
        cmd.Parameters.Add(p2);

        await Task.Run(() => cmd.ExecuteNonQuery(), cancellationToken);
    }

    /// <summary>
    /// Executes the given SQL statement against the database.
    /// </summary>
    private async Task ExecuteSqlAsync(string sql, CancellationToken cancellationToken)
    {
        using var cmd = _dbConnection.CreateCommand();
        cmd.CommandText = sql;
        await Task.Run(() => cmd.ExecuteNonQuery(), cancellationToken);
    }
}