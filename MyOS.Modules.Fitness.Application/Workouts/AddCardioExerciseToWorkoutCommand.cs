using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record AddCardioExerciseToWorkoutCommand(Guid WorkoutId, Guid ExerciseId, int Duration) : ICommand<Guid>;

    public sealed class AddCardioExerciseToWorkoutCommandValidator : AbstractValidator<AddCardioExerciseToWorkoutCommand>
    {
        public AddCardioExerciseToWorkoutCommandValidator()
        {
            RuleFor(x => x.ExerciseId).NotEmpty();
            RuleFor(x => x.Duration).GreaterThan(0);
        }
    }

    internal sealed class AddCardioExerciseToWorkoutCommandHandler(
        IWorkoutRepository workoutRepository,
        IExerciseRepository exerciseRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<AddCardioExerciseToWorkoutCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(AddCardioExerciseToWorkoutCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.WorkoutId, cancellationToken);

            if (workout is null)
                return Result<Guid>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Guid>.Failure(WorkoutErrors.Forbidden);

            var exercise = await exerciseRepository.GetByIdAsync(command.ExerciseId, cancellationToken);
            if (exercise is null || exercise.UserId != currentUser.Id)
                return Result<Guid>.Failure(WorkoutErrors.ExerciseNotFound);

            if (exercise is not CardioExercise)
                return Result<Guid>.Failure(WorkoutErrors.ActivityTypeMismatch);

            if (workout.ContainsExercise(command.ExerciseId))
                return Result<Guid>.Failure(WorkoutErrors.ExerciseAlreadyInWorkout);

            var entry = workout.AddCardioExercise(command.ExerciseId, command.Duration);

            // Track the new entry as Added explicitly (see IWorkoutRepository.AddExercise).
            workoutRepository.AddExercise(entry);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(entry.Id);
        }
    }
}
