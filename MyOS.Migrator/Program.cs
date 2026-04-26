using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, configuration) =>
    {
        configuration.AddUserSecrets<Program>(optional: false);
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

var logger = scope.ServiceProvider
    .GetRequiredService<ILoggerFactory>()
    .CreateLogger("MyOS.Migrator");

var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

try
{
    logger.LogInformation("Starting database migrations");

    runner.MigrateUp();

    logger.LogInformation("Database migrations completed successfully");
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Database migration failed");
    throw;
}