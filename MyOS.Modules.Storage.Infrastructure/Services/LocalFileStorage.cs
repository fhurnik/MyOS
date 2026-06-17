using Microsoft.Extensions.Options;
using MyOS.Modules.Storage.Application.Abstractions;
using MyOS.Modules.Storage.Infrastructure.Settings;

namespace MyOS.Modules.Storage.Infrastructure.Services
{
    /// <summary>
    /// Stores file content on the API server disk under {RootPath}/{userId}/{storageFileName}.
    /// Both path segments are GUID-derived, so no user input reaches the filesystem path.
    /// </summary>
    internal sealed class LocalFileStorage(IOptions<StorageSettings> options) : IFileStorage
    {
        private readonly string _rootPath = options.Value.RootPath;

        public async Task SaveAsync(Guid userId, string storageFileName, Stream content, CancellationToken cancellationToken)
        {
            var userDirectory = GetUserDirectory(userId);
            Directory.CreateDirectory(userDirectory);

            var path = Path.Combine(userDirectory, storageFileName);
            await using var target = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(target, cancellationToken);
        }

        public void Delete(Guid userId, string storageFileName)
        {
            var path = Path.Combine(GetUserDirectory(userId), storageFileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        public bool Exists(Guid userId, string storageFileName) =>
            File.Exists(Path.Combine(GetUserDirectory(userId), storageFileName));

        public string GetAbsolutePath(Guid userId, string storageFileName) =>
            Path.GetFullPath(Path.Combine(GetUserDirectory(userId), storageFileName));

        private string GetUserDirectory(Guid userId) =>
            Path.Combine(_rootPath, userId.ToString("N"));
    }
}
