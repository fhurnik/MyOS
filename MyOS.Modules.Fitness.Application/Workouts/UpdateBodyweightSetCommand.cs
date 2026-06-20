using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record UpdateBodyweightSetCommand(
        Guid WorkoutId, Guid WorkoutExerciseId, Guid SetId, int Reps, decimal? AddedWeight, int? Negatives, byte? Rir) : ICommand<Unit>;

    public sealed class UpdateBodyweightSetCommandValidator : AbstractValidator<UpdateBodyweightSetCommand>
    {
        public UpdateBodyweightSetCommandValidator()
        {
            RuleFor(x => x.Reps).GreaterThan(0);
            RuleFor(x => x.AddedWeight).GreaterThanOrEqualTo(0).When(x => x.AddedWeight.HasValue);
            RuleFor(x => x.Negatives).GreaterThanOrEqualTo(0).When(x => x.Negatives.HasValue);
            RuleFor(x => x.Rir).InclusiveBetween((byte)0, (byte)10).When(x => x.Rir.HasValue);
        }
    }

    internal sealed class UpdateBodyweightSetCommandHandler(
        IWorkoutRepository workoutRepository,
        IExerciseRepository exerciseRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateBodyweightSetCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateBodyweightSetCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.WorkoutId, cancellationToken);

            if (workout is null)
                return Result<Unit>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Unit>.Failure(WorkoutErrors.Forbidden);

            var entry = workout.FindExercise(command.WorkoutExerciseId);
            if (entry is null)
                return Result<Unit>.Failure(WorkoutErrors.WorkoutExerciseNotFound);

            var set = entry.FindSet(command.SetId);
            if (set is null)
                return Result<Unit>.Failure(WorkoutErrors.SetNotFound);

            var exercise = await exerciseRepository.GetByIdAsync(entry.ExerciseId, cancellationToken);
            if (exercise is not StrengthExercise { StrengthCategory: StrengthCategory.Bodyweight })
                return Result<Unit>.Failure(WorkoutErrors.ActivityTypeMismatch);

            set.UpdateBodyweight(command.Reps, command.AddedWeight, command.Negatives, command.Rir);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
