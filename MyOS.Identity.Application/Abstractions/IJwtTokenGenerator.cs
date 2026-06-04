using MyOS.Core.Domain.Enums;

namespace MyOS.Identity.Application.Abstractions
{
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(Guid userId, string email, Language language);
        string GenerateRefreshToken();
    }
}
