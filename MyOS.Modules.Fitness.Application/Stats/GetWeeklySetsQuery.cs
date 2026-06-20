using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Stats.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Fitness.Application.Stats
{
    public sealed record GetWeeklySetsQuery(Guid? ExerciseId) : IQuery<IReadOnlyList<WeeklySetsDto>>;

    internal sealed class GetWeeklySetsQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetWeeklySetsQuery, IReadOnlyList<WeeklySetsDto>>
    {
        public async Task<Result<IReadOnlyList<WeeklySetsDto>>> Handle(GetWeeklySetsQuery query, CancellationToken cancellationToken)
        {
            var baseQuery = db.Query("fitness.v_weekly_sets").Where("user_id", currentUser.Id);

            if (query.ExerciseId.HasValue)
                baseQuery.Where("exercise_id", query.ExerciseId.Value);

            var rows = await baseQuery
                .OrderByDesc("iso_year")
                .OrderByDesc("iso_week")
                .GetAsync<WeeklySetsDto>(cancellationToken: cancellationToken);

            return Result<IReadOnlyList<WeeklySetsDto>>.Success(rows.ToList());
        }
    }
}
