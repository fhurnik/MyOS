using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Application.Exercises
{
    public sealed record UpdateCardioExerciseCommand(Guid Id, string Name, int Distance) : ICommand<Unit>;

    public sealed class UpdateCardioExerciseCommandValidator : AbstractValidator<UpdateCardioExerciseCommand>
    {
        public UpdateCardioExerciseCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Distance).GreaterThan(0);
        }
    }

    internal sealed class UpdateCardioExerciseCommandHandler(
        IExerciseRepository exerciseRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateCardioExerciseCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateCardioExerciseCommand command, CancellationToken cancellationToken)
        {
            var exercise = await exerciseRepository.GetByIdAsync(command.Id, cancellationToken);

            if (exercise is null)
                return Result<Unit>.Failure(ExerciseErrors.NotFound);

            if (exercise.UserId != currentUser.Id)
                return Result<Unit>.Failure(ExerciseErrors.Forbidden);

            if (exercise is not CardioExercise cardio)
                return Result<Unit>.Failure(ExerciseErrors.ActivityTypeMismatch);

            cardio.Rename(command.Name);

            // Distance is locked once the exercise is used in a workout — that rule is enforced
            // from Etap 2 (when IWorkoutRepository exists). Apply only on actual change.
            if (cardio.Distance != command.Distance)
                cardio.ChangeDistance(command.Distance);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
