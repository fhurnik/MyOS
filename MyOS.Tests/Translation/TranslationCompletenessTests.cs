using MyOS.Core.Domain.Enums;

namespace MyOS.Tests.Translation
{
    public sealed class TranslationCompletenessTests
    {
        [Fact]
        public void AllErrorCodes_HaveTranslationsForAllLanguages()
        {
            var codes = ErrorTestFixture.AllErrorFields()
                .Select(x => x.Error.Code)
                .ToList();

            Assert.NotEmpty(codes);

            var missing = new List<string>();
            var languages = Enum.GetValues<Language>();

            foreach (var code in codes)
            {
                foreach (var language in languages)
                {
                    var found = ErrorTestFixture.Providers.Any(p => p.TryGet(code, language) is not null);
                    if (!found)
                        missing.Add($"[{language}] {code}");
                }
            }

            Assert.True(missing.Count == 0,
                $"Missing translations:{Environment.NewLine}{string.Join(Environment.NewLine, missing)}");
        }
    }
}
