using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record GetWorkoutQuery(Guid Id) : IQuery<WorkoutDto>;

    internal sealed class GetWorkoutQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetWorkoutQuery, WorkoutDto>
    {
        public async Task<Result<WorkoutDto>> Handle(GetWorkoutQuery query, CancellationToken cancellationToken)
        {
            var summary = await db.Query("fitness.v_workouts")
                .Where("id", query.Id)
                .Where("user_id", currentUser.Id)
                .FirstOrDefaultAsync<WorkoutSummaryDto>(cancellationToken: cancellationToken);

            if (summary is null)
                return Result<WorkoutDto>.Failure(WorkoutErrors.NotFound);

            var exercises = (await db.Query("fitness.v_workout_exercises")
                .Where("workout_id", query.Id)
                .Where("user_id", currentUser.Id)
                .OrderBy("position")
                .GetAsync<WorkoutExerciseDto>(cancellationToken: cancellationToken)).ToList();

            var sets = (await db.Query("fitness.v_exercise_sets")
                .Where("workout_id", query.Id)
                .Where("user_id", currentUser.Id)
                .OrderBy("position")
                .GetAsync<ExerciseSetDto>(cancellationToken: cancellationToken)).ToList();

            var setsByExercise = sets
                .GroupBy(s => s.WorkoutExerciseId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<ExerciseSetDto>)g.ToList());

            var exerciseDtos = exercises
                .Select(e => e with
                {
                    Sets = setsByExercise.TryGetValue(e.Id, out var s) ? s : []
                })
                .ToList();

            var result = new WorkoutDto(
                summary.Id,
                summary.UserId,
                summary.Date,
                summary.Notes,
                exerciseDtos,
                summary.CreatedAtUtc,
                summary.UpdatedAtUtc);

            return Result<WorkoutDto>.Success(result);
        }
    }
}
