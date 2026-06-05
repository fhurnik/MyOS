namespace MyOS.Modules.Notes.Application.Notes.CheckList.Shared
{
    public sealed record CheckListItemDto
    {
        public Guid Id { get; init; }
        public string Text { get; init; } = string.Empty;
        public bool IsChecked { get; init; }
        public int Order { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
