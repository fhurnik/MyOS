namespace MyOS.Modules.Storage.Domain.Quotas
{
    public interface IStorageQuotaRepository
    {
        Task AddAsync(StorageQuota quota, CancellationToken cancellationToken);

        Task<StorageQuota?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}
