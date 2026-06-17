namespace MyOS.Modules.Storage.Domain.Files
{
    public interface IStoredFileRepository
    {
        Task AddAsync(StoredFile file, CancellationToken cancellationToken);
        Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<StoredFile?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
