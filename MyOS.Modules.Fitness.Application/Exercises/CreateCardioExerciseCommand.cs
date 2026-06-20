using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Domain.Exercises;

namespace MyOS.Modules.Fitness.Application.Exercises
{
    public sealed record CreateCardioExerciseCommand(string Name, int Distance) : ICommand<Guid>;

    public sealed class CreateCardioExerciseCommandValidator : AbstractValidator<CreateCardioExerciseCommand>
    {
        public CreateCardioExerciseCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Distance).GreaterThan(0);
        }
    }

    internal sealed class CreateCardioExerciseCommandHandler(
        IExerciseRepository exerciseRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateCardioExerciseCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateCardioExerciseCommand command, CancellationToken cancellationToken)
        {
            var exercise = CardioExercise.Create(currentUser.Id, command.Name, command.Distance);

            await exerciseRepository.AddAsync(exercise, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(exercise.Id);
        }
    }
}
