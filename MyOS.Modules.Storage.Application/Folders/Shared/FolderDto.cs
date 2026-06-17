namespace MyOS.Modules.Storage.Application.Folders.Shared
{
    public sealed record FolderDto
    {
        public Guid Id { get; init; }
        public Guid? ParentId { get; init; }
        public required string Name { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
