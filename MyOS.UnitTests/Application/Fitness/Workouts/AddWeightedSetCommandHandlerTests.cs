using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    // Reference example of a COMMAND-HANDLER unit test. Repositories, ICurrentUser and
    // IUnitOfWork are substituted; the test exercises every branch the handler can return
    // (one Result.Failure per branch + the happy path) and asserts the save semantics.
    public class AddWeightedSetCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private AddWeightedSetCommandHandler CreateHandler() =>
            new(_workouts, _exercises, _currentUser, _unitOfWork);

        private static AddWeightedSetCommand Command(Guid workoutId, Guid workoutExerciseId) =>
            new(workoutId, workoutExerciseId, Reps: 8, Weight: 100m, Rir: 2);

        [Fact]
        public async Task Handle_WorkoutDoesNotExist_ReturnsNotFoundAndDoesNotSave()
        {
            _workouts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Workout?)null);

            var result = await CreateHandler().Handle(
                Command(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(WorkoutErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WorkoutOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersWorkout = Workout.Create(userId: Guid.NewGuid(), new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(othersWorkout.Id, Arg.Any<CancellationToken>())
                .Returns(othersWorkout);

            var result = await CreateHandler().Handle(
                Command(othersWorkout.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.Forbidden);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WorkoutExerciseNotInWorkout_ReturnsWorkoutExerciseNotFound()
        {
            var workout = Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                Command(workout.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.WorkoutExerciseNotFound);
        }

        [Fact]
        public async Task Handle_ExerciseIsNotWeightedStrength_ReturnsActivityTypeMismatch()
        {
            var workout = Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);
            var entry = workout.AddStrengthExercise(Guid.NewGuid());
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(entry.ExerciseId, Arg.Any<CancellationToken>())
                .Returns(StrengthExercise.Create(_currentUser.Id, "Pull-up", StrengthCategory.Bodyweight));

            var result = await CreateHandler().Handle(
                Command(workout.Id, entry.Id), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ActivityTypeMismatch);
        }

        [Fact]
        public async Task Handle_ValidWeightedSet_AddsSetSavesOnceAndReturnsSetId()
        {
            var workout = Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);
            var entry = workout.AddStrengthExercise(Guid.NewGuid());
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(entry.ExerciseId, Arg.Any<CancellationToken>())
                .Returns(StrengthExercise.Create(_currentUser.Id, "Bench press", StrengthCategory.Weighted));

            var result = await CreateHandler().Handle(
                Command(workout.Id, entry.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            _workouts.Received(1).AddSet(Arg.Any<ExerciseSet>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
