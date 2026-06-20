using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Pagination;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Application.SqlKata;
using MyOS.Modules.Fitness.Application.Workouts.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record GetWorkoutsQuery(PagingRequest Paging) : IQuery<PagingList<WorkoutSummaryDto>>;

    internal sealed class GetWorkoutsQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetWorkoutsQuery, PagingList<WorkoutSummaryDto>>
    {
        public async Task<Result<PagingList<WorkoutSummaryDto>>> Handle(GetWorkoutsQuery query, CancellationToken cancellationToken)
        {
            var baseQuery = db.Query("fitness.v_workouts").Where("user_id", currentUser.Id);

            if (string.IsNullOrEmpty(query.Paging.OrderBy))
                baseQuery.OrderByDesc("date");

            return await baseQuery.GetPagingListAsync<WorkoutSummaryDto>(query.Paging, cancellationToken);
        }
    }
}
