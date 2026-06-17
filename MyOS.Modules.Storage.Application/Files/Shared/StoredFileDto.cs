namespace MyOS.Modules.Storage.Application.Files.Shared
{
    public sealed record StoredFileDto
    {
        public Guid Id { get; init; }
        public string OriginalName { get; init; } = string.Empty;
        public string Extension { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public long SizeBytes { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
        public DateTime? DeletedAtUtc { get; init; }
    }
}
