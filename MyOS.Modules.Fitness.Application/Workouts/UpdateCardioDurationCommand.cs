using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record UpdateCardioDurationCommand(Guid WorkoutId, Guid WorkoutExerciseId, int Duration) : ICommand<Unit>;

    public sealed class UpdateCardioDurationCommandValidator : AbstractValidator<UpdateCardioDurationCommand>
    {
        public UpdateCardioDurationCommandValidator()
        {
            RuleFor(x => x.Duration).GreaterThan(0);
        }
    }

    internal sealed class UpdateCardioDurationCommandHandler(
        IWorkoutRepository workoutRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateCardioDurationCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateCardioDurationCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.WorkoutId, cancellationToken);

            if (workout is null)
                return Result<Unit>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Unit>.Failure(WorkoutErrors.Forbidden);

            var entry = workout.FindExercise(command.WorkoutExerciseId);
            if (entry is null)
                return Result<Unit>.Failure(WorkoutErrors.WorkoutExerciseNotFound);

            // Only cardio entries carry a duration; a strength entry has none.
            if (entry.Duration is null)
                return Result<Unit>.Failure(WorkoutErrors.ActivityTypeMismatch);

            entry.ChangeDuration(command.Duration);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
