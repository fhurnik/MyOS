using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record AddBodyweightSetCommand(
        Guid WorkoutId, Guid WorkoutExerciseId, int Reps, decimal? AddedWeight, int? Negatives, byte? Rir) : ICommand<Guid>;

    public sealed class AddBodyweightSetCommandValidator : AbstractValidator<AddBodyweightSetCommand>
    {
        public AddBodyweightSetCommandValidator()
        {
            RuleFor(x => x.Reps).GreaterThan(0);
            RuleFor(x => x.AddedWeight).GreaterThanOrEqualTo(0).When(x => x.AddedWeight.HasValue);
            RuleFor(x => x.Negatives).GreaterThanOrEqualTo(0).When(x => x.Negatives.HasValue);
            RuleFor(x => x.Rir).InclusiveBetween((byte)0, (byte)10).When(x => x.Rir.HasValue);
        }
    }

    internal sealed class AddBodyweightSetCommandHandler(
        IWorkoutRepository workoutRepository,
        IExerciseRepository exerciseRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<AddBodyweightSetCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(AddBodyweightSetCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.WorkoutId, cancellationToken);

            if (workout is null)
                return Result<Guid>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Guid>.Failure(WorkoutErrors.Forbidden);

            var entry = workout.FindExercise(command.WorkoutExerciseId);
            if (entry is null)
                return Result<Guid>.Failure(WorkoutErrors.WorkoutExerciseNotFound);

            var exercise = await exerciseRepository.GetByIdAsync(entry.ExerciseId, cancellationToken);
            if (exercise is not StrengthExercise { StrengthCategory: StrengthCategory.Bodyweight })
                return Result<Guid>.Failure(WorkoutErrors.ActivityTypeMismatch);

            var set = entry.AddBodyweightSet(command.Reps, command.AddedWeight, command.Negatives, command.Rir);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(set.Id);
        }
    }
}
