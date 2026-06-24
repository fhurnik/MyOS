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
    public sealed record DeleteExerciseCommand(Guid Id) : ICommand<Unit>;

    internal sealed class DeleteExerciseCommandHandler(
        IExerciseRepository exerciseRepository,
        IWorkoutRepository workoutRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<DeleteExerciseCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(DeleteExerciseCommand command, CancellationToken cancellationToken)
        {
            var exercise = await exerciseRepository.GetByIdAsync(command.Id, cancellationToken);

            if (exercise is null)
                return Result<Unit>.Failure(ExerciseErrors.NotFound);

            if (exercise.UserId != currentUser.Id)
                return Result<Unit>.Failure(ExerciseErrors.Forbidden);

            // An exercise referenced by any workout cannot be deleted (would orphan history) —
            // consistent with the immutability rule on type/category changes.
            var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
                new ExerciseMustNotBeInUseRule(workoutRepository, exercise.Id));

            if (check.IsFailure)
                return Result<Unit>.Failure(check.Error);

            exercise.Delete();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
