using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class UpdateWorkoutCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private UpdateWorkoutCommandHandler CreateHandler() =>
            new(_workouts, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_WorkoutDoesNotExist_ReturnsNotFound()
        {
            _workouts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Workout?)null);

            var result = await CreateHandler().Handle(
                new UpdateWorkoutCommand(Guid.NewGuid(), new DateOnly(2026, 6, 21), null),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.NotFound);
        }

        [Fact]
        public async Task Handle_WorkoutOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersWorkout = Workout.Create(Guid.NewGuid(), new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(othersWorkout.Id, Arg.Any<CancellationToken>()).Returns(othersWorkout);

            var result = await CreateHandler().Handle(
                new UpdateWorkoutCommand(othersWorkout.Id, new DateOnly(2026, 6, 22), "x"),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.Forbidden);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_OwnedWorkout_UpdatesDateAndNotesAndSavesOnce()
        {
            var workout = Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), "old");
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            var newDate = new DateOnly(2026, 6, 22);

            var result = await CreateHandler().Handle(
                new UpdateWorkoutCommand(workout.Id, newDate, "new notes"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            workout.Date.ShouldBe(newDate);
            workout.Notes.ShouldBe("new notes");
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
