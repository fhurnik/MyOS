using MyOS.Core.Application.Abstractions;
using MyOS.Core.Domain.Enums;
using System.Resources;

namespace MyOS.Core.Application.Resources
{
    internal sealed class CoreErrorMessageProvider : IErrorMessageProvider
    {
        private static readonly IReadOnlyDictionary<Language, string> LanguageCodes = new Dictionary<Language, string>
        {
            [Language.English] = "en",
            [Language.Polish] = "pl"
        };

        private static readonly IReadOnlyDictionary<Language, ResourceManager> Managers;

        static CoreErrorMessageProvider()
        {
            var assembly = typeof(CoreErrorMessageProvider).Assembly;
            var ns = typeof(CoreErrorMessageProvider).Namespace!;

            Managers = LanguageCodes.ToDictionary(
                kv => kv.Key,
                kv => new ResourceManager($"{ns}.PagingErrors_{kv.Value}", assembly));
        }

        public string? TryGet(string errorCode, Language language) =>
            Managers.TryGetValue(language, out var rm) ? rm.GetString(errorCode) : null;
    }
}
