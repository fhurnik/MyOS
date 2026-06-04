namespace MyOS.Identity.Application.Abstractions
{
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(Guid userId, string email);
        string GenerateRefreshToken();
    }
}
