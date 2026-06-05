namespace MyOS.Modules.Notes.Application.Notes.TextNotes.Shared
{
    public sealed record TextNoteDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Text { get; init; } = string.Empty;
        public DateTime CreatedAtUtc { get; init; }
        public DateTime? UpdatedAtUtc { get; init; }
    }
}
