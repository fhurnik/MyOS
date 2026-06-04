using System.Collections;
using System.Resources;

namespace MyOS.Tests.Translation
{
    public sealed class ResourceKeyConventionTests
    {
        [Fact]
        public void AllResourceKeys_ExistAsErrorCodes()
        {
            var knownCodes = ErrorTestFixture.AllErrorFields()
                .Select(x => x.Error.Code)
                .ToHashSet();

            var orphans = new List<string>();

            foreach (var (assembly, resourceName) in ErrorTestFixture.ResourceManifests)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new ResourceReader(stream);

                foreach (DictionaryEntry entry in reader)
                {
                    var key = (string)entry.Key;
                    if (!knownCodes.Contains(key))
                        orphans.Add($"[{resourceName}] '{key}'");
                }
            }

            Assert.True(orphans.Count == 0,
                $"Orphaned resource keys (no matching error code):{Environment.NewLine}{string.Join(Environment.NewLine, orphans)}");
        }

        [Fact]
        public void AllResourceKeys_HaveCorrectFormat()
        {
            var violations = new List<string>();

            foreach (var (assembly, resourceName) in ErrorTestFixture.ResourceManifests)
            {
                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new ResourceReader(stream);

                foreach (DictionaryEntry entry in reader)
                {
                    var key = (string)entry.Key;
                    var parts = key.Split('.');
                    if (parts.Length != 2 || string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                        violations.Add($"[{resourceName}] '{key}' — expected format '{{ClassName}}.{{FieldName}}'");
                }
            }

            Assert.True(violations.Count == 0,
                $"Resource key format violations:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
        }
    }
}
