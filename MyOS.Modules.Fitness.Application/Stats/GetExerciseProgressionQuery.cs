using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Stats.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Fitness.Application.Stats
{
    public sealed record GetExerciseProgressionQuery(Guid ExerciseId) : IQuery<ProgressionDto>;

    internal sealed class GetExerciseProgressionQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetExerciseProgressionQuery, ProgressionDto>
    {
        public async Task<Result<ProgressionDto>> Handle(GetExerciseProgressionQuery query, CancellationToken cancellationToken)
        {
            var points = (await db.Query("fitness.v_exercise_progression")
                .Where("exercise_id", query.ExerciseId)
                .Where("user_id", currentUser.Id)
                .OrderBy("date")
                .GetAsync<ProgressionPointDto>(cancellationToken: cancellationToken)).ToList();

            var target = await db.Query("fitness.v_exercise_targets")
                .Where("exercise_id", query.ExerciseId)
                .Where("user_id", currentUser.Id)
                .Select("value")
                .FirstOrDefaultAsync<decimal?>(cancellationToken: cancellationToken);

            var result = new ProgressionDto(query.ExerciseId, target, points);

            return Result<ProgressionDto>.Success(result);
        }
    }
}
