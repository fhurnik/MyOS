using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Targets;

namespace MyOS.Modules.Fitness.Application.Targets
{
    // PUT upsert: one target per exercise, the single metric value matching the progression.
    public sealed record SetExerciseTargetCommand(Guid ExerciseId, decimal Value) : ICommand<Unit>;

    public sealed class SetExerciseTargetCommandValidator : AbstractValidator<SetExerciseTargetCommand>
    {
        public SetExerciseTargetCommandValidator()
        {
            RuleFor(x => x.Value).GreaterThan(0);
        }
    }

    internal sealed class SetExerciseTargetCommandHandler(
        IExerciseRepository exerciseRepository,
        IExerciseTargetRepository targetRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<SetExerciseTargetCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(SetExerciseTargetCommand command, CancellationToken cancellationToken)
        {
            var exercise = await exerciseRepository.GetByIdAsync(command.ExerciseId, cancellationToken);

            if (exercise is null)
                return Result<Unit>.Failure(ExerciseErrors.NotFound);

            if (exercise.UserId != currentUser.Id)
                return Result<Unit>.Failure(ExerciseErrors.Forbidden);

            var target = await targetRepository.GetByExerciseIdAsync(command.ExerciseId, cancellationToken);

            if (target is null)
            {
                target = ExerciseTarget.Create(command.ExerciseId, currentUser.Id, command.Value);
                await targetRepository.AddAsync(target, cancellationToken);
            }
            else
            {
                target.ChangeValue(command.Value);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
