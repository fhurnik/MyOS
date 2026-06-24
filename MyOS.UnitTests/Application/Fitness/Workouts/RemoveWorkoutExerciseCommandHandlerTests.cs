using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class RemoveWorkoutExerciseCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private RemoveWorkoutExerciseCommandHandler CreateHandler() =>
            new(_workouts, _currentUser, _unitOfWork);

        private Workout OwnedWorkout() =>
            Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);

        [Fact]
        public async Task Handle_WorkoutDoesNotExist_ReturnsNotFound()
        {
            _workouts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Workout?)null);

            var result = await CreateHandler().Handle(
                new RemoveWorkoutExerciseCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.NotFound);
        }

        [Fact]
        public async Task Handle_WorkoutOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersWorkout = Workout.Create(Guid.NewGuid(), new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(othersWorkout.Id, Arg.Any<CancellationToken>()).Returns(othersWorkout);

            var result = await CreateHandler().Handle(
                new RemoveWorkoutExerciseCommand(othersWorkout.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_WorkoutExerciseNotInWorkout_ReturnsWorkoutExerciseNotFound()
        {
            var workout = OwnedWorkout();
            workout.AddStrengthExercise(Guid.NewGuid());
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                new RemoveWorkoutExerciseCommand(workout.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.WorkoutExerciseNotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ExistingWorkoutExercise_RemovesAndSavesOnce()
        {
            var workout = OwnedWorkout();
            var entry = workout.AddStrengthExercise(Guid.NewGuid());
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                new RemoveWorkoutExerciseCommand(workout.Id, entry.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            workout.FindExercise(entry.Id).ShouldBeNull();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
