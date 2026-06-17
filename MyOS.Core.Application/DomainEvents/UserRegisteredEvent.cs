using MediatR;

namespace MyOS.Core.Application.DomainEvents
{
    /// <summary>
    /// Published when a new user is registered. Consumed in-process by other modules
    /// (e.g. Storage creates the user's storage quota) within the same transaction as
    /// the registration — handlers must NOT call SaveChanges themselves.
    /// </summary>
    public sealed record UserRegisteredEvent(Guid UserId) : INotification;
}
