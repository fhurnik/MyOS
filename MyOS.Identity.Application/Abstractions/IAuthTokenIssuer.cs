using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Application.Abstractions
{
    public interface IAuthTokenIssuer
    {
        Task<AuthTokens> IssueAsync(User user, CancellationToken cancellationToken);
    }
}
