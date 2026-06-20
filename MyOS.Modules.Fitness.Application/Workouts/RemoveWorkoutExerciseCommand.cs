using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record RemoveWorkoutExerciseCommand(Guid WorkoutId, Guid WorkoutExerciseId) : ICommand<Unit>;

    internal sealed class RemoveWorkoutExerciseCommandHandler(
        IWorkoutRepository workoutRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<RemoveWorkoutExerciseCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(RemoveWorkoutExerciseCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.WorkoutId, cancellationToken);

            if (workout is null)
                return Result<Unit>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Unit>.Failure(WorkoutErrors.Forbidden);

            if (!workout.RemoveExercise(command.WorkoutExerciseId))
                return Result<Unit>.Failure(WorkoutErrors.WorkoutExerciseNotFound);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
