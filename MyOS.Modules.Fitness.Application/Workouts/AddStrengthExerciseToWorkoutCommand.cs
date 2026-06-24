using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts.Shared;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record AddStrengthExerciseToWorkoutCommand(
        Guid WorkoutId,
        Guid ExerciseId,
        IReadOnlyList<SetInput> Sets) : ICommand<Guid>;

    public sealed class AddStrengthExerciseToWorkoutCommandValidator : AbstractValidator<AddStrengthExerciseToWorkoutCommand>
    {
        public AddStrengthExerciseToWorkoutCommandValidator()
        {
            RuleFor(x => x.ExerciseId).NotEmpty();
            RuleFor(x => x.Sets).NotEmpty();
            RuleForEach(x => x.Sets).ChildRules(set =>
            {
                set.RuleFor(s => s.Reps).GreaterThan(0);
                set.RuleFor(s => s.Weight).GreaterThanOrEqualTo(0).When(s => s.Weight.HasValue);
                set.RuleFor(s => s.AddedWeight).GreaterThanOrEqualTo(0).When(s => s.AddedWeight.HasValue);
                set.RuleFor(s => s.Rir).InclusiveBetween((byte)0, (byte)10).When(s => s.Rir.HasValue);
                set.RuleFor(s => s.Negatives).GreaterThanOrEqualTo(0).When(s => s.Negatives.HasValue);
            });
        }
    }

    internal sealed class AddStrengthExerciseToWorkoutCommandHandler(
        IWorkoutRepository workoutRepository,
        IExerciseRepository exerciseRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<AddStrengthExerciseToWorkoutCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(AddStrengthExerciseToWorkoutCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.WorkoutId, cancellationToken);

            if (workout is null)
                return Result<Guid>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Guid>.Failure(WorkoutErrors.Forbidden);

            var exercise = await exerciseRepository.GetByIdAsync(command.ExerciseId, cancellationToken);
            if (exercise is null || exercise.UserId != currentUser.Id)
                return Result<Guid>.Failure(WorkoutErrors.ExerciseNotFound);

            if (exercise is not StrengthExercise strength)
                return Result<Guid>.Failure(WorkoutErrors.ActivityTypeMismatch);

            if (workout.ContainsExercise(command.ExerciseId))
                return Result<Guid>.Failure(WorkoutErrors.ExerciseAlreadyInWorkout);

            // Each inline set must match the exercise category: weighted carries Weight only,
            // bodyweight carries AddedWeight/Negatives only.
            var setsMismatch = strength.StrengthCategory == StrengthCategory.Weighted
                ? command.Sets.Any(s => s.Weight is null || s.AddedWeight is not null || s.Negatives is not null)
                : command.Sets.Any(s => s.Weight is not null);

            if (setsMismatch)
                return Result<Guid>.Failure(WorkoutErrors.ActivityTypeMismatch);

            var entry = workout.AddStrengthExercise(command.ExerciseId);

            foreach (var set in command.Sets)
            {
                if (strength.StrengthCategory == StrengthCategory.Weighted)
                    entry.AddWeightedSet(set.Reps, set.Weight!.Value, set.Rir);
                else
                    entry.AddBodyweightSet(set.Reps, set.AddedWeight, set.Negatives, set.Rir);
            }

            // Force the new entry + its sets to Added (see IWorkoutRepository.AddExercise).
            workoutRepository.AddExercise(entry);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(entry.Id);
        }
    }
}
