using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.UnitTests.Domain.Storage.Quotas
{
    // The only decision in StorageQuota is Release's floor-at-zero guard; Consume is plumbing
    // but is asserted here once to anchor the used-bytes accounting the guard depends on.
    public class StorageQuotaTests
    {
        private static StorageQuota NewQuota(long maxBytes = 1000) =>
            StorageQuota.Create(userId: Guid.NewGuid(), maxBytes: maxBytes);

        [Fact]
        public void Consume_AddsToUsedBytes()
        {
            var quota = NewQuota();

            quota.Consume(300);
            quota.Consume(200);

            quota.UsedBytes.ShouldBe(500);
        }

        [Fact]
        public void Release_LessThanUsed_SubtractsExactly()
        {
            var quota = NewQuota();
            quota.Consume(500);

            quota.Release(200);

            quota.UsedBytes.ShouldBe(300);
        }

        [Fact]
        public void Release_MoreThanUsed_FloorsAtZero()
        {
            var quota = NewQuota();
            quota.Consume(100);

            // Releasing more than was consumed must never drive UsedBytes negative.
            quota.Release(500);

            quota.UsedBytes.ShouldBe(0);
        }
    }
}
