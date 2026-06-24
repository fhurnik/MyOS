using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record UpdateWorkoutCommand(Guid Id, DateOnly Date, string? Notes) : ICommand<Unit>;

    public sealed class UpdateWorkoutCommandValidator : AbstractValidator<UpdateWorkoutCommand>
    {
        public UpdateWorkoutCommandValidator()
        {
            RuleFor(x => x.Date).LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow));
            RuleFor(x => x.Notes).MaximumLength(2000);
        }
    }

    internal sealed class UpdateWorkoutCommandHandler(
        IWorkoutRepository workoutRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UpdateWorkoutCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(UpdateWorkoutCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.Id, cancellationToken);

            if (workout is null)
                return Result<Unit>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Unit>.Failure(WorkoutErrors.Forbidden);

            workout.Update(command.Date, command.Notes);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
