using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Pagination;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Application.SqlKata;
using MyOS.Modules.Notes.Application.Notes.CheckList.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record GetCheckListsQuery(PagingRequest Paging) : IQuery<PagingList<CheckListSummaryDto>>;

    internal sealed class GetCheckListsQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetCheckListsQuery, PagingList<CheckListSummaryDto>>
    {
        public async Task<Result<PagingList<CheckListSummaryDto>>> Handle(GetCheckListsQuery query, CancellationToken cancellationToken)
        {
            var result = await db.Query("notes.v_check_lists")
                .Where("user_id", currentUser.Id)
                .OrderByDesc("created_at_utc")
                .GetPagingListAsync<CheckListSummaryDto>(query.Paging, cancellationToken);

            return Result<PagingList<CheckListSummaryDto>>.Success(result);
        }
    }
}
