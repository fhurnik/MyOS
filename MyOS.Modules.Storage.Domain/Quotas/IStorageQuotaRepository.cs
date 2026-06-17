namespace MyOS.Modules.Storage.Domain.Quotas
{
    public interface IStorageQuotaRepository
    {
        Task AddAsync(StorageQuota quota, CancellationToken cancellationToken);
    }
}
