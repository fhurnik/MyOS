using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Exercises;

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

            // StrengthCategory is locked once the exercise is used in a workout — that rule is
            // enforced from Etap 2 (when IWorkoutRepository exists). Apply only on actual change.
            if (strength.StrengthCategory != command.Category)
                strength.ChangeCategory(command.Category);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
