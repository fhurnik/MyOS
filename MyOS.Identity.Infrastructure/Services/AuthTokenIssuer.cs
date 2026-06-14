using Microsoft.Extensions.Options;
using MyOS.Identity.Application.Abstractions;
using MyOS.Identity.Application.Commands.Shared;
using MyOS.Identity.Domain.Users;

namespace MyOS.Identity.Infrastructure.Services
{
    internal sealed class AuthTokenIssuer(
        IJwtTokenGenerator jwtTokenGenerator,
        IUserRepository userRepository,
        IOptions<JwtSettings> jwtSettings) : IAuthTokenIssuer
    {
        public async Task<AuthTokens> IssueAsync(User user, CancellationToken cancellationToken)
        {
            var accessToken = jwtTokenGenerator.GenerateAccessToken(user.Id, user.Email, user.Language);
            var rawRefreshToken = jwtTokenGenerator.GenerateRefreshToken();
            var expiresAt = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpiryDays);

            var refreshToken = RefreshToken.Create(user.Id, rawRefreshToken, expiresAt);
            await userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);

            return new AuthTokens(accessToken, rawRefreshToken);
        }
    }
}
