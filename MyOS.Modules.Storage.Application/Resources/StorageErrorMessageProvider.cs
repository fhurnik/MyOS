using MyOS.Core.Application.Abstractions;
using MyOS.Core.Domain.Enums;
using System.Resources;

namespace MyOS.Modules.Storage.Application.Resources
{
    internal sealed class StorageErrorMessageProvider : IErrorMessageProvider
    {
        private static readonly IReadOnlyDictionary<Language, string> LanguageCodes = new Dictionary<Language, string>
        {
            [Language.English] = "en",
            [Language.Polish] = "pl"
        };

        private static readonly IReadOnlyDictionary<Language, ResourceManager> Managers;

        static StorageErrorMessageProvider()
        {
            var assembly = typeof(StorageErrorMessageProvider).Assembly;
            var ns = typeof(StorageErrorMessageProvider).Namespace!;

            Managers = LanguageCodes.ToDictionary(
                kv => kv.Key,
                kv => new ResourceManager($"{ns}.StorageErrors_{kv.Value}", assembly));
        }

        public string? TryGet(string errorCode, Language language) =>
            Managers.TryGetValue(language, out var rm) ? rm.GetString(errorCode) : null;
    }
}
