using MediatR;
using MyOS.Core.Application.DomainEvents;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Application.Quotas.EventHandlers
{
    /// <summary>
    /// Creates the default storage quota for a newly registered user. Runs in-process within the
    /// Identity registration transaction — only stages the entity, never calls SaveChanges
    /// (the Register command handler's single SaveChanges commits user + quota atomically).
    /// </summary>
    internal sealed class CreateQuotaOnUserRegisteredHandler(
        IStorageQuotaRepository quotaRepository) : INotificationHandler<UserRegisteredEvent>
    {
        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            var quota = StorageQuota.Create(notification.UserId);
            await quotaRepository.AddAsync(quota, cancellationToken);
        }
    }
}
