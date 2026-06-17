using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Abstractions;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Infrastructure.Settings;

namespace MyOS.Modules.Storage.Infrastructure.Services
{
    // Periodically purges files that have been soft-deleted longer than the retention period:
    // removes the bytes from disk and the metadata row from the database. Quota was already
    // freed at soft-delete time, so it is not touched here.
    internal sealed class StorageCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<StorageSettings> options,
        ILogger<StorageCleanupBackgroundService> logger) : BackgroundService
    {
        private readonly StorageSettings _settings = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromHours(_settings.CleanupIntervalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PurgeExpiredFilesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Storage cleanup run failed");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task PurgeExpiredFilesAsync(CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var fileRepository = scope.ServiceProvider.GetRequiredService<IStoredFileRepository>();
            var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var cutoff = DateTime.UtcNow.AddDays(-_settings.RetentionDays);
            var expired = await fileRepository.GetSoftDeletedBeforeAsync(cutoff, cancellationToken);

            if (expired.Count == 0)
                return;

            // Delete bytes first (idempotent), then drop the rows. If SaveChanges fails the rows
            // survive and the next run retries — Delete tolerates an already-missing file.
            foreach (var file in expired)
                fileStorage.Delete(file.UserId, file.StorageFileName);

            fileRepository.RemoveRange(expired);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Storage cleanup purged {Count} expired files", expired.Count);
        }
    }
}
