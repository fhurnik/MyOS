using Microsoft.EntityFrameworkCore;
using MyOS.Core.Infrastructure.EntityFrameworkConfiguration;
using MyOS.Modules.Notes.Domain.Notes.TextNotes;

namespace MyOS.Modules.Notes.Infrastructure.Repositories
{
    internal sealed class TextNoteRepository(AppDbContext dbContext) : ITextNoteRepository
    {
        public Task<TextNote?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            dbContext.Set<TextNote>()
                .FirstOrDefaultAsync(n => n.Id == id && n.DeletedAtUtc == null, cancellationToken);

        public async Task AddAsync(TextNote note, CancellationToken cancellationToken) =>
            await dbContext.Set<TextNote>().AddAsync(note, cancellationToken);
    }
}
