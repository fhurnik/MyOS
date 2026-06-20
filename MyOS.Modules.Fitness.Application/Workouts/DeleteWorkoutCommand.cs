using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Domain.Workouts;

namespace MyOS.Modules.Fitness.Application.Workouts
{
    public sealed record DeleteWorkoutCommand(Guid Id) : ICommand<Unit>;

    internal sealed class DeleteWorkoutCommandHandler(
        IWorkoutRepository workoutRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<DeleteWorkoutCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(DeleteWorkoutCommand command, CancellationToken cancellationToken)
        {
            var workout = await workoutRepository.GetByIdAsync(command.Id, cancellationToken);

            if (workout is null)
                return Result<Unit>.Failure(WorkoutErrors.NotFound);

            if (workout.UserId != currentUser.Id)
                return Result<Unit>.Failure(WorkoutErrors.Forbidden);

            workout.Delete();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
