using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class UpdateWeightedSetCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private UpdateWeightedSetCommandHandler CreateHandler() =>
            new(_workouts, _exercises, _currentUser, _unitOfWork);

        private (Workout workout, WorkoutExercise entry, ExerciseSet set) OwnedWorkoutWithSet()
        {
            var workout = Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);
            var entry = workout.AddStrengthExercise(Guid.NewGuid());
            var set = entry.AddWeightedSet(8, 100m, 2);
            return (workout, entry, set);
        }

        private static UpdateWeightedSetCommand Command(Guid workoutId, Guid entryId, Guid setId) =>
            new(workoutId, entryId, setId, Reps: 10, Weight: 110m, Rir: 1);

        [Fact]
        public async Task Handle_WorkoutDoesNotExist_ReturnsNotFound()
        {
            _workouts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Workout?)null);

            var result = await CreateHandler().Handle(
                Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.NotFound);
        }

        [Fact]
        public async Task Handle_WorkoutOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersWorkout = Workout.Create(Guid.NewGuid(), new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(othersWorkout.Id, Arg.Any<CancellationToken>()).Returns(othersWorkout);

            var result = await CreateHandler().Handle(
                Command(othersWorkout.Id, Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_WorkoutExerciseNotInWorkout_ReturnsWorkoutExerciseNotFound()
        {
            var (workout, _, set) = OwnedWorkoutWithSet();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                Command(workout.Id, Guid.NewGuid(), set.Id), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.WorkoutExerciseNotFound);
        }

        [Fact]
        public async Task Handle_SetNotInExercise_ReturnsSetNotFound()
        {
            var (workout, entry, _) = OwnedWorkoutWithSet();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                Command(workout.Id, entry.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.SetNotFound);
        }

        [Fact]
        public async Task Handle_ExerciseIsNotWeightedStrength_ReturnsActivityTypeMismatch()
        {
            var (workout, entry, set) = OwnedWorkoutWithSet();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(entry.ExerciseId, Arg.Any<CancellationToken>())
                .Returns(StrengthExercise.Create(_currentUser.Id, "Pull-up", StrengthCategory.Bodyweight));

            var result = await CreateHandler().Handle(
                Command(workout.Id, entry.Id, set.Id), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ActivityTypeMismatch);
        }

        [Fact]
        public async Task Handle_ValidWeightedSet_UpdatesSetAndSavesOnce()
        {
            var (workout, entry, set) = OwnedWorkoutWithSet();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(entry.ExerciseId, Arg.Any<CancellationToken>())
                .Returns(StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted));

            var result = await CreateHandler().Handle(
                Command(workout.Id, entry.Id, set.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            set.Reps.ShouldBe(10);
            set.Weight.ShouldBe(110m);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
