using MyOS.Core.Application.Abstractions;
using MyOS.Core.Domain.Enums;
using System.Resources;

namespace MyOS.Modules.Fitness.Application.Resources
{
    internal sealed class FitnessErrorMessageProvider : IErrorMessageProvider
    {
        private static readonly IReadOnlyDictionary<Language, string> LanguageCodes = new Dictionary<Language, string>
        {
            [Language.English] = "en",
            [Language.Polish] = "pl"
        };

        private static readonly IReadOnlyDictionary<Language, ResourceManager> Managers;

        static FitnessErrorMessageProvider()
        {
            var assembly = typeof(FitnessErrorMessageProvider).Assembly;
            var ns = typeof(FitnessErrorMessageProvider).Namespace!;

            Managers = LanguageCodes.ToDictionary(
                kv => kv.Key,
                kv => new ResourceManager($"{ns}.FitnessErrors_{kv.Value}", assembly));
        }

        public string? TryGet(string errorCode, Language language) =>
            Managers.TryGetValue(language, out var rm) ? rm.GetString(errorCode) : null;
    }
}
