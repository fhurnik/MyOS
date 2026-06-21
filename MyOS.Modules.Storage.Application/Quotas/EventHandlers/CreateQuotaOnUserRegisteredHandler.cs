using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.DomainEvents;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Application.Quotas.EventHandlers
{
    /// <summary>
    /// Creates the default storage quota for a newly registered user. Runs after the user has been
    /// committed by the Register command handler, then persists the quota in its own SaveChanges —
    /// the quota's foreign key to identity.users requires the user row to already exist.
    /// </summary>
    internal sealed class CreateQuotaOnUserRegisteredHandler(
        IStorageQuotaRepository quotaRepository,
        IUnitOfWork unitOfWork) : INotificationHandler<UserRegisteredEvent>
    {
        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            var quota = StorageQuota.Create(notification.UserId);
            await quotaRepository.AddAsync(quota, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
