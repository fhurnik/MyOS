using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Pagination;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Application.SqlKata;
using MyOS.Modules.Notes.Application.Notes.TextNotes.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Notes.Application.Notes.TextNotes
{
    public sealed record GetTextNotesQuery(PagingRequest Paging) : IQuery<PagingList<TextNoteDto>>;

    internal sealed class GetTextNotesQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetTextNotesQuery, PagingList<TextNoteDto>>
    {
        public async Task<Result<PagingList<TextNoteDto>>> Handle(GetTextNotesQuery query, CancellationToken cancellationToken)
        {
            var result = await db.Query("notes.v_text_notes")
                .Where("user_id", currentUser.Id)
                //.OrderBy("title")
                .GetPagingListAsync<TextNoteDto>(query.Paging, cancellationToken);

            return Result<PagingList<TextNoteDto>>.Success(result);
        }
    }
}
