using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Storage.Domain.Files;

namespace MyOS.Modules.Storage.Infrastructure.Repositories
{
    internal sealed class StoredFileRepository(AppDbContext dbContext) : IStoredFileRepository
    {
        public async Task AddAsync(StoredFile file, CancellationToken cancellationToken) =>
            await dbContext.Set<StoredFile>().AddAsync(file, cancellationToken);

        public Task<StoredFile?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Set<StoredFile>()
                .FirstOrDefaultAsync(f => f.Id == id && f.DeletedAtUtc == null, cancellationToken);

        public Task<StoredFile?> GetDeletedByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Set<StoredFile>()
                .FirstOrDefaultAsync(f => f.Id == id && f.DeletedAtUtc != null, cancellationToken);

        public async Task<IReadOnlyList<StoredFile>> GetActiveByFolderIdsAsync(
            IReadOnlyCollection<Guid> folderIds, CancellationToken cancellationToken) =>
            await dbContext.Set<StoredFile>()
                .Where(f => f.FolderId != null && folderIds.Contains(f.FolderId.Value) && f.DeletedAtUtc == null)
                .ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<StoredFile>> GetSoftDeletedBeforeAsync(
            DateTime cutoffUtc, CancellationToken cancellationToken) =>
            await dbContext.Set<StoredFile>()
                .Where(f => f.DeletedAtUtc != null && f.DeletedAtUtc < cutoffUtc)
                .ToListAsync(cancellationToken);

        public void RemoveRange(IReadOnlyCollection<StoredFile> files) =>
            dbContext.Set<StoredFile>().RemoveRange(files);
    }
}
