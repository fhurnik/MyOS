using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Storage.Domain.Folders;

namespace MyOS.Modules.Storage.Infrastructure.Repositories
{
    internal sealed class FolderRepository(AppDbContext dbContext) : IFolderRepository
    {
        public async Task AddAsync(Folder folder, CancellationToken cancellationToken) =>
            await dbContext.Set<Folder>().AddAsync(folder, cancellationToken);

        public Task<Folder?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Set<Folder>()
                .FirstOrDefaultAsync(f => f.Id == id && f.DeletedAtUtc == null, cancellationToken);

        public async Task<IReadOnlyList<Folder>> GetActiveSubtreeAsync(Guid rootId, CancellationToken cancellationToken)
        {
            // Recursive CTE walks the folder tree from the root down. Executed as-is (no LINQ
            // composition after FromSqlRaw), so the WITH clause stays valid. Entities are tracked,
            // so callers can mutate them and SaveChanges once.
            const string sql =
                """
                WITH descendants AS (
                    SELECT id FROM storage.folders WHERE id = {0}
                    UNION ALL
                    SELECT f.id FROM storage.folders f
                    INNER JOIN descendants d ON f.parent_id = d.id
                )
                SELECT * FROM storage.folders
                WHERE id IN (SELECT id FROM descendants) AND deleted_at_utc IS NULL
                """;

            return await dbContext.Set<Folder>()
                .FromSqlRaw(sql, rootId)
                .ToListAsync(cancellationToken);
        }
    }
}
