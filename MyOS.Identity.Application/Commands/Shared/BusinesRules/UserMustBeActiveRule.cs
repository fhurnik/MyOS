using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Application.Commands.Shared.BusinesRules
{
    internal sealed class UserMustBeActiveRule(User? user) : IBusinessRule
    {
        public Error Error => UserErrors.AccountDisabled;

        public Task<bool> CheckAsync(CancellationToken cancellationToken) 
            => Task.FromResult(user is not null && user.IsActive);
    }
}
