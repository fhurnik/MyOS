using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class RemoveSetCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private RemoveSetCommandHandler CreateHandler() =>
            new(_workouts, _currentUser, _unitOfWork);

        private (Workout workout, WorkoutExercise entry, ExerciseSet set) OwnedWorkoutWithSet()
        {
            var workout = Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);
            var entry = workout.AddStrengthExercise(Guid.NewGuid());
            var set = entry.AddWeightedSet(8, 100m, 2);
            entry.AddWeightedSet(10, 110m, 1); // second set, so removing the first one is allowed
            return (workout, entry, set);
        }

        [Fact]
        public async Task Handle_WorkoutDoesNotExist_ReturnsNotFound()
        {
            _workouts.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Workout?)null);

            var result = await CreateHandler().Handle(
                new RemoveSetCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.NotFound);
        }

        [Fact]
        public async Task Handle_WorkoutOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersWorkout = Workout.Create(Guid.NewGuid(), new DateOnly(2026, 6, 21), null);
            _workouts.GetByIdAsync(othersWorkout.Id, Arg.Any<CancellationToken>()).Returns(othersWorkout);

            var result = await CreateHandler().Handle(
                new RemoveSetCommand(othersWorkout.Id, Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_WorkoutExerciseNotInWorkout_ReturnsWorkoutExerciseNotFound()
        {
            var (workout, _, set) = OwnedWorkoutWithSet();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                new RemoveSetCommand(workout.Id, Guid.NewGuid(), set.Id), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.WorkoutExerciseNotFound);
        }

        [Fact]
        public async Task Handle_SetNotInExercise_ReturnsSetNotFound()
        {
            var (workout, entry, _) = OwnedWorkoutWithSet();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                new RemoveSetCommand(workout.Id, entry.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.SetNotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_RemovingTheOnlySet_ReturnsLastSetCannotBeRemoved()
        {
            var workout = Workout.Create(_currentUser.Id, new DateOnly(2026, 6, 21), null);
            var entry = workout.AddStrengthExercise(Guid.NewGuid());
            var onlySet = entry.AddWeightedSet(8, 100m, 2);
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                new RemoveSetCommand(workout.Id, entry.Id, onlySet.Id), CancellationToken.None);

            result.Error.ShouldBe(WorkoutErrors.LastSetCannotBeRemoved);
            entry.FindSet(onlySet.Id).ShouldNotBeNull(); // not removed
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ExistingSet_RemovesAndSavesOnce()
        {
            var (workout, entry, set) = OwnedWorkoutWithSet();
            _workouts.GetByIdAsync(workout.Id, Arg.Any<CancellationToken>()).Returns(workout);

            var result = await CreateHandler().Handle(
                new RemoveSetCommand(workout.Id, entry.Id, set.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            entry.FindSet(set.Id).ShouldBeNull();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
