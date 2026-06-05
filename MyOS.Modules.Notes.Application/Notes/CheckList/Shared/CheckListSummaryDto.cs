namespace MyOS.Modules.Notes.Application.Notes.CheckList.Shared
{
    public sealed record CheckListSummaryDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
