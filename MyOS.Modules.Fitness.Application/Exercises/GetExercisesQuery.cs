using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Pagination;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Application.SqlKata;
using MyOS.Modules.Fitness.Application.Exercises.Shared;
using MyOS.Modules.Fitness.Domain.Exercises;
using SqlKata.Execution;

namespace MyOS.Modules.Fitness.Application.Exercises
{
    public sealed record GetExercisesQuery(
        PagingRequest Paging,
        ActivityType? ActivityType,
        StrengthCategory? StrengthCategory) : IQuery<PagingList<ExerciseDto>>;

    internal sealed class GetExercisesQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetExercisesQuery, PagingList<ExerciseDto>>
    {
        public async Task<Result<PagingList<ExerciseDto>>> Handle(GetExercisesQuery query, CancellationToken cancellationToken)
        {
            var baseQuery = db.Query("fitness.v_exercises").Where("user_id", currentUser.Id);

            if (query.ActivityType.HasValue)
                baseQuery.Where("activity_type", (byte)query.ActivityType.Value);

            if (query.StrengthCategory.HasValue)
                baseQuery.Where("strength_category", (byte)query.StrengthCategory.Value);

            if (string.IsNullOrEmpty(query.Paging.OrderBy))
                baseQuery.OrderBy("name");

            return await baseQuery.GetPagingListAsync<ExerciseDto>(query.Paging, cancellationToken);
        }
    }
}
