using MyOS.Core.Domain.Entities;

namespace MyOS.Modules.Storage.Domain.Files
{
    public class StoredFile : Entity
    {
        public Guid UserId { get; private set; }
        public string StorageFileName { get; private set; }
        public string OriginalName { get; private set; }
        public string Extension { get; private set; }
        public string ContentType { get; private set; }
        public long SizeBytes { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public DateTime? DeletedAtUtc { get; private set; }

        public static StoredFile Create(
            Guid userId, string originalName, string extension, string contentType, long sizeBytes) =>
            new(userId, originalName, extension, contentType, sizeBytes);

        internal StoredFile(Guid userId, string originalName, string extension, string contentType, long sizeBytes)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            StorageFileName = Id.ToString("N");
            OriginalName = originalName;
            Extension = extension;
            ContentType = contentType;
            SizeBytes = sizeBytes;
            CreatedAtUtc = DateTime.UtcNow;
        }

        internal void Delete()
        {
            DeletedAtUtc = DateTime.UtcNow;
        }

        internal void Restore()
        {
            DeletedAtUtc = null;
            UpdatedAtUtc = DateTime.UtcNow;
        }

        private StoredFile()
        {
            // for EF Core
            StorageFileName = null!;
            OriginalName = null!;
            Extension = null!;
            ContentType = null!;
        }
    }
}
