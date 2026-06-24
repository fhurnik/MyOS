using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files.BusinesRules;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.UnitTests.Application.Storage.Files.BusinessRules
{
    public class QuotaMustHaveSpaceRuleTests
    {
        [Fact]
        public async Task CheckAsync_NullQuota_Fails() =>
            (await new QuotaMustHaveSpaceRule(null, 10).CheckAsync(CancellationToken.None)).ShouldBeFalse();

        [Fact]
        public async Task CheckAsync_RequestExactlyFits_Passes()
        {
            var quota = StorageQuota.Create(Guid.NewGuid(), maxBytes: 100);

            (await new QuotaMustHaveSpaceRule(quota, 100).CheckAsync(CancellationToken.None)).ShouldBeTrue();
        }

        [Fact]
        public async Task CheckAsync_RequestExceeds_Fails()
        {
            var quota = StorageQuota.Create(Guid.NewGuid(), maxBytes: 100);

            (await new QuotaMustHaveSpaceRule(quota, 101).CheckAsync(CancellationToken.None)).ShouldBeFalse();
        }

        [Fact]
        public void Error_IsInsufficientSpace() =>
            new QuotaMustHaveSpaceRule(null, 1).Error.ShouldBe(QuotaErrors.InsufficientSpace);
    }
}
