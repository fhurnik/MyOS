using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Notes.Domain.Notes.CheckList;

namespace MyOS.Modules.Notes.Infrastructure.Repositories
{
    internal sealed class CheckListRepository(AppDbContext dbContext) : ICheckListRepository
    {
        public Task<CheckList?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Set<CheckList>()
                .Include(cl => cl.Items)
                .FirstOrDefaultAsync(cl => cl.Id == id && cl.DeletedAtUtc == null, cancellationToken);

        public async Task AddAsync(CheckList checkList, CancellationToken cancellationToken) =>
            await dbContext.Set<CheckList>().AddAsync(checkList, cancellationToken);

        public async Task AddItemAsync(CheckListItem item, CancellationToken cancellationToken) =>
            await dbContext.Set<CheckListItem>().AddAsync(item, cancellationToken);
    }
}
