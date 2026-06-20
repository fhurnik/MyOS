using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Exercises.BusinesRules;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Exercises
{
    public sealed record UpdateStrengthExerciseCommand(Guid Id, string Name, StrengthCategory Category) : ICommand<Unit>;

    public sealed class UpdateStrengthExerciseCommandValidator : AbstractValidator<UpdateStrengthExerciseCommand>
    {
        public UpdateStrengthExerciseCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Category).IsInEnum();
        }
    }

    internal sealed class UpdateStrengthExerciseCommandHandler(
        IExerciseRepository exerciseRepository,
        IWorkoutRepository workoutRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateStrengthExerciseCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateStrengthExerciseCommand command, CancellationToken cancellationToken)
        {
            var exercise = await exerciseRepository.GetByIdAsync(command.Id, cancellationToken);

            if (exercise is null)
                return Result<Unit>.Failure(ExerciseErrors.NotFound);

            if (exercise.UserId != currentUser.Id)
                return Result<Unit>.Failure(ExerciseErrors.Forbidden);

            if (exercise is not StrengthExercise strength)
                return Result<Unit>.Failure(ExerciseErrors.ActivityTypeMismatch);

            strength.Rename(command.Name);

            // StrengthCategory is locked once the exercise is used in any workout — check only on actual change.
            if (strength.StrengthCategory != command.Category)
            {
                var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
                    new ExerciseMustNotBeInUseRule(workoutRepository, strength.Id));

                if (check.IsFailure)
                    return Result<Unit>.Failure(check.Error);

                strength.ChangeCategory(command.Category);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
