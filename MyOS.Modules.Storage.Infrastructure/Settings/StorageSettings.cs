namespace MyOS.Modules.Storage.Infrastructure.Settings
{
    public sealed class StorageSettings
    {
        public const string SectionName = "Storage";

        // Root directory on the API server where user files are stored.
        public string RootPath { get; set; } = string.Empty;

        // How long a soft-deleted file is kept before the cleanup service purges it permanently.
        public int RetentionDays { get; set; } = 30;

        // How often the cleanup service runs.
        public int CleanupIntervalHours { get; set; } = 24;
    }
}
