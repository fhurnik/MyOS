using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record CreateWorkoutCommand(DateOnly Date, string? Notes) : ICommand<Guid>;

    public sealed class CreateWorkoutCommandValidator : AbstractValidator<CreateWorkoutCommand>
    {
        public CreateWorkoutCommandValidator()
        {
            RuleFor(x => x.Date).LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow));
            RuleFor(x => x.Notes).MaximumLength(2000);
        }
    }

    internal sealed class CreateWorkoutCommandHandler(
        IWorkoutRepository workoutRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateWorkoutCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateWorkoutCommand command, CancellationToken cancellationToken)
        {
            var workout = Workout.Create(currentUser.Id, command.Date, command.Notes);

            await workoutRepository.AddAsync(workout, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(workout.Id);
        }
    }
}
