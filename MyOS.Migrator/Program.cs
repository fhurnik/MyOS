using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, configuration) =>
    {
        if (context.HostingEnvironment.IsDevelopment())
        {
            configuration.AddUserSecrets<Program>(optional: false);
        }
    })
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("Database");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Database' was not found.");
        }

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner =>
            {
                runner
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(Program).Assembly)
                    .For.Migrations();
            })
            .AddLogging(logging =>
            {
                logging.AddFluentMigratorConsole();
            });
    })
    .Build();

using var scope = host.Services.CreateScope();

var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger("MyOS.Migrator");
var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

try
{
    logger.LogInformation("Starting database migrations");
    runner.MigrateUp();
    logger.LogInformation("Database migrations completed successfully");

    // Only update views if migrations succeeded
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("Database");
    var viewsRoot = Path.Combine(AppContext.BaseDirectory, "Views");

    using var dbConnection = new SqlConnection(connectionString);
    var viewLogger = loggerFactory.CreateLogger<SqlViewSynchronizer>();
    var synchronizer = new SqlViewSynchronizer(viewLogger, dbConnection, viewsRoot);

    logger.LogInformation("Starting SQL view synchronization");
    synchronizer.SynchronizeAsync(CancellationToken.None).GetAwaiter().GetResult();
    logger.LogInformation("SQL view synchronization completed successfully");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Database migration failed");
    throw;
}