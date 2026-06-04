using Microsoft.AspNetCore.Http;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyOS.Core.Infrastructure.Services
{
    internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
    {
        private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public Guid Id
        {
            get
            {
                if (!IsAuthenticated)
                    throw new InvalidOperationException("Cannot access user Id for an unauthenticated request.");

                var value = User!.FindFirstValue(JwtRegisteredClaimNames.Sub);
                return Guid.Parse(value!);
            }
        }

        public string Email
        {
            get
            {
                if (!IsAuthenticated)
                    throw new InvalidOperationException("Cannot access user Email for an unauthenticated request.");

                return User!.FindFirstValue(JwtRegisteredClaimNames.Email)!;
            }
        }

        public Language Language
        {
            get
            {
                var value = User?.FindFirstValue("language");
                return value is not null && Enum.TryParse<Language>(value, out var language)
                    ? language
                    : Language.English;
            }
        }
    }
}
