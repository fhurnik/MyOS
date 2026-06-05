using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Application.Notes.TextNotes.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Notes.Application.Notes.TextNotes
{
    public sealed record GetTextNoteQuery(Guid Id) : IQuery<TextNoteDto>;

    internal sealed class GetTextNoteQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetTextNoteQuery, TextNoteDto>
    {
        public async Task<Result<TextNoteDto>> Handle(GetTextNoteQuery query, CancellationToken cancellationToken)
        {
            var note = await db.Query("notes.v_text_notes")
                .Where("id", query.Id)
                .Where("user_id", currentUser.Id)
                .FirstOrDefaultAsync<TextNoteDto>(cancellationToken: cancellationToken);

            if (note is null)
                return Result<TextNoteDto>.Failure(TextNoteErrors.NotFound);

            return Result<TextNoteDto>.Success(note);
        }
    }
}
