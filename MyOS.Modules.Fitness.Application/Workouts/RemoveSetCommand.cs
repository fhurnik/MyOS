using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record RemoveSetCommand(Guid WorkoutId, Guid WorkoutExerciseId, Guid SetId) : ICommand<Unit>;

    internal sealed class RemoveSetCommandHandler(
        IWorkoutRepository workoutRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<RemoveSetCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(RemoveSetCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.WorkoutId, cancellationToken);

            if (workout is null)
                return Result<Unit>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Unit>.Failure(WorkoutErrors.Forbidden);

            var entry = workout.FindExercise(command.WorkoutExerciseId);
            if (entry is null)
                return Result<Unit>.Failure(WorkoutErrors.WorkoutExerciseNotFound);

            if (!entry.RemoveSet(command.SetId))
                return Result<Unit>.Failure(WorkoutErrors.SetNotFound);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
