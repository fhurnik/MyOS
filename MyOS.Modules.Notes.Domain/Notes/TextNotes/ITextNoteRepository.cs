namespace MyOS.Modules.Notes.Domain.Notes.TextNotes
{
    public interface ITextNoteRepository
    {
        Task<TextNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task AddAsync(TextNote note, CancellationToken cancellationToken);
    }
}
