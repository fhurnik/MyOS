using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Application.Commands.Shared.BusinesRules
{
    internal sealed class RefreshTokenMustBeActiveRule(RefreshToken? refreshToken, Guid? expectedUserId = null)
        : IBusinessRule
    {
        public Error Error => UserErrors.InvalidRefreshToken;

        public Task<bool> CheckAsync(CancellationToken cancellationToken)
        {
            var isValid = refreshToken is not null
                && refreshToken.IsActive
                && (expectedUserId is null || refreshToken.UserId == expectedUserId);

            return Task.FromResult(isValid);
        }
    }
}
