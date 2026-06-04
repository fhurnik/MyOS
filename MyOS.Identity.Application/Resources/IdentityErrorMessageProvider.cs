using MyOS.Core.Application.Abstractions;
using MyOS.Core.Domain.Enums;
using System.Resources;

namespace MyOS.Identity.Application.Resources
{
    internal sealed class IdentityErrorMessageProvider : IErrorMessageProvider
    {
        private static readonly IReadOnlyDictionary<Language, string> LanguageCodes = new Dictionary<Language, string>
        {
            [Language.English] = "en",
            [Language.Polish] = "pl"
        };

        private static readonly IReadOnlyDictionary<Language, ResourceManager> Managers;

        static IdentityErrorMessageProvider()
        {
            var assembly = typeof(IdentityErrorMessageProvider).Assembly;
            var ns = typeof(IdentityErrorMessageProvider).Namespace!;

            Managers = LanguageCodes.ToDictionary(
                kv => kv.Key,
                kv => new ResourceManager($"{ns}.IdentityErrors_{kv.Value}", assembly));
        }

        public string? TryGet(string errorCode, Language language) =>
            Managers.TryGetValue(language, out var rm) ? rm.GetString(errorCode) : null;
    }
}
