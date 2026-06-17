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

        /// <summary>Reserves space for an uploaded file. Caller must verify available space first.</summary>
        internal void Consume(long bytes)
        {
            UsedBytes += bytes;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        /// <summary>Frees space when a file is removed. Never drops below zero.</summary>
        internal void Release(long bytes)
        {
            UsedBytes = Math.Max(0, UsedBytes - bytes);
            UpdatedAtUtc = DateTime.UtcNow;
        }

        private StorageQuota() { }
    }
}
