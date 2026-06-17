namespace MyOS.Modules.Storage.Domain.Files
{
    public interface IStoredFileRepository
    {
        Task AddAsync(StoredFile file, CancellationToken cancellationToken);
        Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<StoredFile?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<StoredFile>> GetActiveByFolderIdsAsync(
            IReadOnlyCollection<Guid> folderIds, CancellationToken cancellationToken);

        // Soft-deleted files whose deletion is older than the cutoff — used by the cleanup service.
        Task<IReadOnlyList<StoredFile>> GetSoftDeletedBeforeAsync(
            DateTime cutoffUtc, CancellationToken cancellationToken);

        void RemoveRange(IReadOnlyCollection<StoredFile> files);
    }
}
