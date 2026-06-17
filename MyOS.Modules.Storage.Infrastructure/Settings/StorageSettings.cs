namespace MyOS.Modules.Storage.Infrastructure.Settings
{
    public sealed class StorageSettings
    {
        public const string SectionName = "Storage";

        /// <summary>Root directory on the API server where user files are stored.</summary>
        public string RootPath { get; set; } = string.Empty;
    }
}
