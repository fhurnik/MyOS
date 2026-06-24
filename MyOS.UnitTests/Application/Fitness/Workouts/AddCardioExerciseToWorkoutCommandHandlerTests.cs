using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class AddCardioExerciseToWorkoutCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private AddCardioExerciseToWorkoutCommandHandler CreateHandler() =>
            new(_workouts, _exercises, _currentUser, _unitOfWork);

        private Workout OwnedWorkout() =>
            Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);

        [Fact]
        public async Task Handle_WorkoutDoesNotExist_ReturnsNotFound()
        {
            _workouts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Workout?)null);

            var result = await CreateHandler().Handle(
                new AddCardioExerciseToWorkoutCommand(Guid.NewGuid(), Guid.NewGuid(), 600),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.NotFound);
        }

        [Fact]
        public async Task Handle_WorkoutOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersWorkout = Workout.Create(Guid.NewGuid(), new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(othersWorkout.Id, Arg.Any<CancellationToken>()).Returns(othersWorkout);

            var result = await CreateHandler().Handle(
                new AddCardioExerciseToWorkoutCommand(othersWorkout.Id, Guid.NewGuid(), 600),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_ExerciseDoesNotExist_ReturnsExerciseNotFound()
        {
            var workout = OwnedWorkout();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Exercise?)null);

            var result = await CreateHandler().Handle(
                new AddCardioExerciseToWorkoutCommand(workout.Id, Guid.NewGuid(), 600),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ExerciseNotFound);
        }

        [Fact]
        public async Task Handle_ExerciseOwnedByAnotherUser_ReturnsExerciseNotFound()
        {
            var workout = OwnedWorkout();
            var othersExercise = CardioExercise.Create(Guid.NewGuid(), "Run", 5000);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(othersExercise.Id, Arg.Any<CancellationToken>()).Returns(othersExercise);

            var result = await CreateHandler().Handle(
                new AddCardioExerciseToWorkoutCommand(workout.Id, othersExercise.Id, 600),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ExerciseNotFound);
        }

        [Fact]
        public async Task Handle_ExerciseIsNotCardio_ReturnsActivityTypeMismatch()
        {
            var workout = OwnedWorkout();
            var strength = StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(strength.Id, Arg.Any<CancellationToken>()).Returns(strength);

            var result = await CreateHandler().Handle(
                new AddCardioExerciseToWorkoutCommand(workout.Id, strength.Id, 600),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ActivityTypeMismatch);
        }

        [Fact]
        public async Task Handle_ExerciseAlreadyInWorkout_ReturnsExerciseAlreadyInWorkout()
        {
            var workout = OwnedWorkout();
            var cardio = CardioExercise.Create(_currentUser.Id, "Run", 5000);
            workout.AddCardioExercise(cardio.Id, duration: 600); // exercise already present once
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(cardio);

            var result = await CreateHandler().Handle(
                new AddCardioExerciseToWorkoutCommand(workout.Id, cardio.Id, 600),
                CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.ExerciseAlreadyInWorkout);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ValidCardioExercise_AddsEntrySavesOnceAndReturnsEntryId()
        {
            var workout = OwnedWorkout();
            var cardio = CardioExercise.Create(_currentUser.Id, "Run", 5000);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);
            _exercises.GetByIdAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(cardio);

            var result = await CreateHandler().Handle(
                new AddCardioExerciseToWorkoutCommand(workout.Id, cardio.Id, 600),
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            _workouts.Received(1).AddExercise(Arg.Any<WorkoutExercise>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
