using Microsoft.AspNetCore.Http;
using MyOS.Core.Domain.Enums;
using System.Globalization;
using System.Security.Claims;

namespace MyOS.Core.Infrastructure.Localization
{
    public sealed class LanguageCultureMiddleware(RequestDelegate next)
    {
        private static readonly Dictionary<Language, CultureInfo> Cultures = new()
        {
            [Language.English] = new CultureInfo("en"),
            [Language.Polish] = new CultureInfo("pl")
        };

        public async Task InvokeAsync(HttpContext context)
        {
            var language = GetLanguageFromClaims(context.User);
            var culture = Cultures.GetValueOrDefault(language, Cultures[Language.English]);

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            await next(context);
        }

        private static Language GetLanguageFromClaims(ClaimsPrincipal? user)
        {
            var value = user?.FindFirstValue("language");
            return value is not null && Enum.TryParse<Language>(value, out var language)
                ? language
                : Language.English;
        }
    }
}
