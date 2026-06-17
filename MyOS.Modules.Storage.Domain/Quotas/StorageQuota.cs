using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Storage.Domain.Quotas
{
    public class StorageQuota : Entity
    {
        /// <summary>Default storage limit granted to every user: 5 GB.</summary>
        public const long DefaultMaxBytes = 5L * 1024 * 1024 * 1024;

        public Guid UserId { get; private set; }
        public long MaxBytes { get; private set; }
        public long UsedBytes { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }

        public static StorageQuota Create(Guid userId, long maxBytes = DefaultMaxBytes) =>
            new(userId, maxBytes);

        internal StorageQuota(Guid userId, long maxBytes)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            MaxBytes = maxBytes;
            UsedBytes = 0;
            CreatedAtUtc = DateTime.UtcNow;
        }

        private StorageQuota() { }
    }
}
