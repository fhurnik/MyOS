namespace MyOS.Tests.Translation
{
    public sealed class ErrorCodeConventionTests
    {
        [Fact]
        public void AllErrorFields_HaveCodeMatchingClassAndFieldName()
        {
            var violations = new List<string>();

            foreach (var (type, field, error) in ErrorTestFixture.AllErrorFields())
            {
                var expected = $"{type.Name}.{field.Name}";
                if (error.Code != expected)
                    violations.Add($"{type.Name}.{field.Name}: expected '{expected}', got '{error.Code}'");
            }

            Assert.True(violations.Count == 0,
                $"Error code convention violations:{Environment.NewLine}{string.Join(Environment.NewLine, violations)}");
        }
    }
}
