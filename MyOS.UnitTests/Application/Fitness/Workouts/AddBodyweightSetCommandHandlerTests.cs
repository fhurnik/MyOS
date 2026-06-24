using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class AddBodyweightSetCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private AddBodyweightSetCommandHandler CreateHandler() =>
            new(_workouts, _exercises, _currentUser, _unitOfWork);

        private (Workout workout, WorkoutExercise entry) OwnedWorkoutWithEntry()
        {
            var workout = Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);
            var entry = workout.AddStrengthExercise(Guid.NewGuid());
            return (workout, entry);
        }

        private static AddBodyweightSetCommand Command(Guid workoutId, Guid workoutExerciseId) =>
            new(workoutId, workoutExerciseId, Reps: 10, AddedWeight: 5m, Negatives: null, Rir: 1);

        [Fact]
        public async Task Handle_WorkoutDoesNotExist_ReturnsNotFound()
        {
            _workouts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Workout?)null);

            var result = await CreateHandler().Handle(
                Command(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.NotFound);
        }

        [Fact]
        public async Task Handle_WorkoutOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersWorkout = Workout.Create(Guid.NewGuid(), new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(othersWorkout.Id, Arg.Any<CancellationToken>()).Returns(othersWorkout);

            var result = await CreateHandler().Handle(
                Command(othersWorkout.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_WorkoutExerciseNotInWorkout_ReturnsWorkoutExerciseNotFound()
        {
            var (workout, _) = OwnedWorkoutWithEntry();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                Command(workout.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.WorkoutExerciseNotFound);
        }

        [Fact]
        public async Task Handle_ExerciseIsNotBodyweightStrength_ReturnsActivityTypeMismatch()
        {
            var (workout, entry) = OwnedWorkoutWithEntry();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(entry.ExerciseId, Arg.Any<CancellationToken>())
                .Returns(StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted));

            var result = await CreateHandler().Handle(
                Command(workout.Id, entry.Id), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ActivityTypeMismatch);
        }

        [Fact]
        public async Task Handle_ValidBodyweightSet_AddsSetSavesOnceAndReturnsSetId()
        {
            var (workout, entry) = OwnedWorkoutWithEntry();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(entry.ExerciseId, Arg.Any<CancellationToken>())
                .Returns(StrengthExercise.Create(_currentUser.Id, "Pull-up", StrengthCategory.Bodyweight));

            var result = await CreateHandler().Handle(
                Command(workout.Id, entry.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            _workouts.Received(1).AddSet(Arg.Any<ExerciseSet>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
