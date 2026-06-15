using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Identity.Application.Errors;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Application.Commands.Register.BusinesRules
{
    internal sealed class EmailMustBeUniqueRule(IUserRepository userRepository, string email)
        : IBusinessRule
    {
        public Error Error => UserErrors.EmailAlreadyInUse with
        {
            Parameters = new Dictionary<string, string> { ["email"] = email }
        };

        public async Task<bool> CheckAsync(CancellationToken cancellationToken)
        {
            var existing = await userRepository.GetByEmailAsync(email, cancellationToken);
            return existing is null;
        }
    }
}
