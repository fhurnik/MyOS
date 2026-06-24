using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Exercises;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Exercises
{
    public class DeleteExerciseCommandHandlerTests
    {
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private DeleteExerciseCommandHandler CreateHandler() =>
            new(_exercises, _workouts, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_ExerciseDoesNotExist_ReturnsNotFoundAndDoesNotSave()
        {
            _exercises.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Exercise?)null);

            var result = await CreateHandler().Handle(
                new DeleteExerciseCommand(Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ExerciseOwnedByAnotherUser_ReturnsForbiddenAndDoesNotSave()
        {
            var othersExercise = CardioExercise.Create(userId: Guid.NewGuid(), "Run", 5000);
            _exercises.GetByIdAsync(othersExercise.Id, Arg.Any<CancellationToken>())
                .Returns(othersExercise);

            var result = await CreateHandler().Handle(
                new DeleteExerciseCommand(othersExercise.Id), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.Forbidden);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_OwnedExercise_SoftDeletesAndSavesOnce()
        {
            var exercise = CardioExercise.Create(_currentUser.Id, "Run", 5000);
            _exercises.GetByIdAsync(exercise.Id, Arg.Any<CancellationToken>()).Returns(exercise);

            var result = await CreateHandler().Handle(
                new DeleteExerciseCommand(exercise.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            exercise.DeletedAtUtc.ShouldNotBeNull(); // soft delete, never a physical remove
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ExerciseUsedInAWorkout_ReturnsInUseAndDoesNotDelete()
        {
            var exercise = CardioExercise.Create(_currentUser.Id, "Run", 5000);
            _exercises.GetByIdAsync(exercise.Id, Arg.Any<CancellationToken>()).Returns(exercise);
            _workouts.ExistsByExerciseAsync(exercise.Id, Arg.Any<CancellationToken>()).Returns(true);

            var result = await CreateHandler().Handle(
                new DeleteExerciseCommand(exercise.Id), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.InUse);
            exercise.DeletedAtUtc.ShouldBeNull(); // not deleted
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
