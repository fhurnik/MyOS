namespace MyOS.Modules.Storage.Domain.Folders
{
    public interface IFolderRepository
    {
        Task AddAsync(Folder folder, CancellationToken cancellationToken);

        Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

        // The folder plus all of its (non-deleted) descendants — used for cascade delete and cycle checks.
        Task<IReadOnlyList<Folder>> GetActiveSubtreeAsync(Guid rootId, CancellationToken cancellationToken);
    }
}
