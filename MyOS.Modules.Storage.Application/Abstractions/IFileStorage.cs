namespace MyOS.Modules.Storage.Application.Abstractions
{
    /// <summary>
    /// Physical storage of file content, abstracted away from the domain. Default implementation
    /// writes to the API server disk; can be swapped for blob storage (S3/MinIO/Azure) later.
    /// </summary>
    public interface IFileStorage
    {
        Task SaveAsync(Guid userId, string storageFileName, Stream content, CancellationToken cancellationToken);

        void Delete(Guid userId, string storageFileName);

        bool Exists(Guid userId, string storageFileName);

        /// <summary>Absolute path on disk — used to stream a download response.</summary>
        string GetAbsolutePath(Guid userId, string storageFileName);
    }
}
