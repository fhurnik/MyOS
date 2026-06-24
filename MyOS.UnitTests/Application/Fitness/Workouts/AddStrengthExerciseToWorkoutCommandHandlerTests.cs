using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Application.Workouts.Shared;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class AddStrengthExerciseToWorkoutCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private AddStrengthExerciseToWorkoutCommandHandler CreateHandler() =>
            new(_workouts, _exercises, _currentUser, _unitOfWork);

        private Workout OwnedWorkout() =>
            Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);

        private static IReadOnlyList<SetInput> WeightedSets() =>
            [new SetInput(Reps: 8, Weight: 100m, AddedWeight: null, Negatives: null, Rir: 2)];

        [Fact]
        public async Task Handle_WorkoutDoesNotExist_ReturnsNotFound()
        {
            _workouts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Workout?)null);

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(Guid.NewGuid(), Guid.NewGuid(), WeightedSets()),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.NotFound);
        }

        [Fact]
        public async Task Handle_WorkoutOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersWorkout = Workout.Create(Guid.NewGuid(), new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(othersWorkout.Id, Arg.Any<CancellationToken>()).Returns(othersWorkout);

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(othersWorkout.Id, Guid.NewGuid(), WeightedSets()),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_ExerciseMissingOrNotOwned_ReturnsExerciseNotFound()
        {
            var workout = OwnedWorkout();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Exercise?)null);

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(workout.Id, Guid.NewGuid(), WeightedSets()),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ExerciseNotFound);
        }

        [Fact]
        public async Task Handle_ExerciseIsNotStrength_ReturnsActivityTypeMismatch()
        {
            var workout = OwnedWorkout();
            var cardio = CardioExercise.Create(_currentUser.Id, "Run", 5000);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(cardio);

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(workout.Id, cardio.Id, WeightedSets()),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ActivityTypeMismatch);
        }

        [Fact]
        public async Task Handle_WeightedExerciseWithSetMissingWeight_ReturnsActivityTypeMismatch()
        {
            var workout = OwnedWorkout();
            var weighted = StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(weighted.Id, Arg.Any<CancellationToken>()).Returns(weighted);
            IReadOnlyList<SetInput> setWithoutWeight =
                [new SetInput(Reps: 8, Weight: null, AddedWeight: null, Negatives: null, Rir: null)];

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(workout.Id, weighted.Id, setWithoutWeight),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ActivityTypeMismatch);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ExerciseAlreadyInWorkout_ReturnsExerciseAlreadyInWorkout()
        {
            var workout = OwnedWorkout();
            var weighted = StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted);
            workout.AddStrengthExercise(weighted.Id); // exercise already present once
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(weighted.Id, Arg.Any<CancellationToken>()).Returns(weighted);

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(workout.Id, weighted.Id, WeightedSets()),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ExerciseAlreadyInWorkout);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WeightedSetCarryingBodyweightFields_ReturnsActivityTypeMismatch()
        {
            var workout = OwnedWorkout();
            var weighted = StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(weighted.Id, Arg.Any<CancellationToken>()).Returns(weighted);
            IReadOnlyList<SetInput> sets =
                [new SetInput(Reps: 8, Weight: 100m, AddedWeight: 5m, Negatives: null, Rir: null)];

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(workout.Id, weighted.Id, sets),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ActivityTypeMismatch);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_BodyweightSetCarryingWeight_ReturnsActivityTypeMismatch()
        {
            var workout = OwnedWorkout();
            var bodyweight = StrengthExercise.Create(_currentUser.Id, "Pull-up", StrengthCategory.Bodyweight);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(bodyweight.Id, Arg.Any<CancellationToken>()).Returns(bodyweight);
            IReadOnlyList<SetInput> sets =
                [new SetInput(Reps: 8, Weight: 50m, AddedWeight: null, Negatives: null, Rir: null)];

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(workout.Id, bodyweight.Id, sets),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ActivityTypeMismatch);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ValidWeightedExerciseWithSets_AddsEntrySavesOnceAndReturnsEntryId()
        {
            var workout = OwnedWorkout();
            var weighted = StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(weighted.Id, Arg.Any<CancellationToken>()).Returns(weighted);

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(workout.Id, weighted.Id, WeightedSets()),
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            _workouts.Received(1).AddExercise(Arg.Any<WorkoutExercise>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ValidBodyweightExerciseWithSets_AddsEntryAndSavesOnce()
        {
            var workout = OwnedWorkout();
            var bodyweight = StrengthExercise.Create(_currentUser.Id, "Pull-up", StrengthCategory.Bodyweight);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(bodyweight.Id, Arg.Any<CancellationToken>()).Returns(bodyweight);
            IReadOnlyList<SetInput> bodyweightSets =
                [new SetInput(Reps: 10, Weight: null, AddedWeight: 5m, Negatives: null, Rir: 1)];

            var result = await CreateHandler().Handle(
                new AddStrengthExerciseToWorkoutCommand(workout.Id, bodyweight.Id, bodyweightSets),
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
