using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Application.Files.BusinesRules
{
    /// <summary>
    /// Passes when the user's quota can accommodate <paramref name="requestedBytes"/> more bytes.
    /// </summary>
    internal sealed class QuotaMustHaveSpaceRule(StorageQuota? quota, long requestedBytes) : IBusinessRule
    {
        public Error Error => QuotaErrors.InsufficientSpace;

        public Task<bool> CheckAsync(CancellationToken cancellationToken) =>
            Task.FromResult(quota is not null && quota.UsedBytes + requestedBytes <= quota.MaxBytes);
    }
}
