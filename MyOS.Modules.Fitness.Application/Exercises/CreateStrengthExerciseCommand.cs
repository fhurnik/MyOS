using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Application.Exercises
{
    public sealed record CreateStrengthExerciseCommand(string Name, StrengthCategory Category) : ICommand<Guid>;

    public sealed class CreateStrengthExerciseCommandValidator : AbstractValidator<CreateStrengthExerciseCommand>
    {
        public CreateStrengthExerciseCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Category).IsInEnum();
        }
    }

    internal sealed class CreateStrengthExerciseCommandHandler(
        IExerciseRepository exerciseRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateStrengthExerciseCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateStrengthExerciseCommand command, CancellationToken cancellationToken)
        {
            var exercise = StrengthExercise.Create(currentUser.Id, command.Name, command.Category);

            await exerciseRepository.AddAsync(exercise, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(exercise.Id);
        }
    }
}
