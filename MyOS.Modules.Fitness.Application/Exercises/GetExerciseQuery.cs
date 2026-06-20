using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Exercises.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Fitness.Application.Exercises
{
    public sealed record GetExerciseQuery(Guid Id) : IQuery<ExerciseDto>;

    internal sealed class GetExerciseQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetExerciseQuery, ExerciseDto>
    {
        public async Task<Result<ExerciseDto>> Handle(GetExerciseQuery query, CancellationToken cancellationToken)
        {
            var exercise = await db.Query("fitness.v_exercises")
                .Where("id", query.Id)
                .Where("user_id", currentUser.Id)
                .FirstOrDefaultAsync<ExerciseDto>(cancellationToken: cancellationToken);

            if (exercise is null)
                return Result<ExerciseDto>.Failure(ExerciseErrors.NotFound);

            return Result<ExerciseDto>.Success(exercise);
        }
    }
}
