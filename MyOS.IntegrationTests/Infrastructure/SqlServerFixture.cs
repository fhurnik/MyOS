using Dapper;
using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SqlKata.Compilers;
using SqlKata.Execution;
using Testcontainers.MsSql;

namespace MyOS.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Spins up a real SQL Server in a container, applies the actual FluentMigrator migrations and
    /// SQL view definitions, and hands out a <see cref="QueryFactory"/> wired exactly like the app
    /// (SqlServerCompiler + Dapper underscore matching). Shared across the test collection so the
    /// container starts once.
    /// </summary>
    public sealed class SqlServerFixture : IAsyncLifetime
    {
        private const string DatabaseName = "MyOS_Test";

        private readonly MsSqlContainer _container = new MsSqlBuilder().Build();

        public string ConnectionString { get; private set; } = string.Empty;

        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            // The container exposes only 'master'; create a dedicated application database and
            // migrate into it, mirroring how the app is deployed.
            var masterConnectionString = _container.GetConnectionString();
            await CreateDatabaseAsync(masterConnectionString);

            ConnectionString = new SqlConnectionStringBuilder(masterConnectionString)
            {
                InitialCatalog = DatabaseName
            }.ConnectionString;

            // Same global Dapper setting the app applies in AddCore() — required for snake_case
            // columns (user_id) to map onto PascalCase DTO properties (UserId).
            DefaultTypeMap.MatchNamesWithUnderscores = true;

            RunMigrations(ConnectionString);
            await SynchronizeViewsAsync(ConnectionString);
        }

        public QueryFactory CreateQueryFactory() =>
            new(new SqlConnection(ConnectionString), new SqlServerCompiler());

        public async Task DisposeAsync() => await _container.DisposeAsync();

        private async Task CreateDatabaseAsync(string masterConnectionString)
        {
            await using var connection = new SqlConnection(masterConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"IF DB_ID('{DatabaseName}') IS NULL CREATE DATABASE [{DatabaseName}];";
            await command.ExecuteNonQueryAsync();
        }

        private static void RunMigrations(string connectionString)
        {
            // ScanIn the Migrator assembly via a public type living in it (SqlViewSynchronizer).
            using var provider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(runner => runner
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(SqlViewSynchronizer).Assembly).For.Migrations())
                .AddLogging(logging => logging.AddFluentMigratorConsole())
                .BuildServiceProvider(validateScopes: false);

            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();
        }

        private static async Task SynchronizeViewsAsync(string connectionString)
        {
            var viewsRoot = Path.Combine(AppContext.BaseDirectory, "Views");

            using var connection = new SqlConnection(connectionString);
            var synchronizer = new SqlViewSynchronizer(
                NullLogger<SqlViewSynchronizer>.Instance, connection, viewsRoot);

            await synchronizer.SynchronizeAsync(CancellationToken.None);
        }
    }
}
