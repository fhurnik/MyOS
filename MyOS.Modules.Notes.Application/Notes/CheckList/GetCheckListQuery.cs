using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Application.Notes.CheckList.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Notes.Application.Notes.CheckList
{
    public sealed record GetCheckListQuery(Guid Id) : IQuery<CheckListDto>;

    internal sealed class GetCheckListQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetCheckListQuery, CheckListDto>
    {
        public async Task<Result<CheckListDto>> Handle(GetCheckListQuery query, CancellationToken cancellationToken)
        {
            var summary = await db.Query("notes.v_check_lists")
                .Where("id", query.Id)
                .Where("user_id", currentUser.Id)
                .FirstOrDefaultAsync<CheckListSummaryDto>(cancellationToken: cancellationToken);

            if (summary is null)
                return Result<CheckListDto>.Failure(CheckListErrors.NotFound);

            var items = await db.Query("notes.v_check_list_items")
                .Where("check_list_id", query.Id)
                .Where("user_id", currentUser.Id)
                .OrderBy("order")
                .GetAsync<CheckListItemDto>(cancellationToken: cancellationToken);

            var result = new CheckListDto(
                summary.Id,
                summary.UserId,
                summary.Title,
                items.ToList().AsReadOnly(),
                summary.CreatedAtUtc,
                summary.UpdatedAtUtc);

            return Result<CheckListDto>.Success(result);
        }
    }
}
