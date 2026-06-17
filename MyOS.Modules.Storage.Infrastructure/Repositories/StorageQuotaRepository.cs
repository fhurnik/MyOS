using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Infrastructure.Repositories
{
    internal sealed class StorageQuotaRepository(AppDbContext dbContext) : IStorageQuotaRepository
    {
        public async Task AddAsync(StorageQuota quota, CancellationToken cancellationToken) =>
            await dbContext.Set<StorageQuota>().AddAsync(quota, cancellationToken);
    }
}
