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
    public sealed record UpdateWeightedSetCommand(
        Guid WorkoutId, Guid WorkoutExerciseId, Guid SetId, int Reps, decimal Weight, byte? Rir) : ICommand<Unit>;

    public sealed class UpdateWeightedSetCommandValidator : AbstractValidator<UpdateWeightedSetCommand>
    {
        public UpdateWeightedSetCommandValidator()
        {
            RuleFor(x => x.Reps).GreaterThan(0);
            RuleFor(x => x.Weight).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Rir).InclusiveBetween((byte)0, (byte)10).When(x => x.Rir.HasValue);
        }
    }

    internal sealed class UpdateWeightedSetCommandHandler(
        IWorkoutRepository workoutRepository,
        IExerciseRepository exerciseRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateWeightedSetCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateWeightedSetCommand command, CancellationToken cancellationToken)
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
            if (exercise is not StrengthExercise { StrengthCategory: StrengthCategory.Weighted })
                return Result<Unit>.Failure(WorkoutErrors.ActivityTypeMismatch);

            set.UpdateWeighted(command.Reps, command.Weight, command.Rir);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
