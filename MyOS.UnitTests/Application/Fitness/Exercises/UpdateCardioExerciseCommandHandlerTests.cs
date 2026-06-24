using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Exercises;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Exercises
{
    public class UpdateCardioExerciseCommandHandlerTests
    {
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private UpdateCardioExerciseCommandHandler CreateHandler() =>
            new(_exercises, _workouts, _currentUser, _unitOfWork);

        private CardioExercise OwnedCardio(int distance = 5000) =>
            CardioExercise.Create(_currentUser.Id, "Run", distance);

        [Fact]
        public async Task Handle_ExerciseDoesNotExist_ReturnsNotFound()
        {
            _exercises.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Exercise?)null);

            var result = await CreateHandler().Handle(
                new UpdateCardioExerciseCommand(Guid.NewGuid(), "Run", 5000), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.NotFound);
        }

        [Fact]
        public async Task Handle_ExerciseOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersCardio = CardioExercise.Create(userId: Guid.NewGuid(), "Run", 5000);
            _exercises.GetByIdAsync(othersCardio.Id, Arg.Any<CancellationToken>()).Returns(othersCardio);

            var result = await CreateHandler().Handle(
                new UpdateCardioExerciseCommand(othersCardio.Id, "Run", 5000), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_ExerciseIsNotCardio_ReturnsActivityTypeMismatch()
        {
            var strength = StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted);
            _exercises.GetByIdAsync(strength.Id, Arg.Any<CancellationToken>()).Returns(strength);

            var result = await CreateHandler().Handle(
                new UpdateCardioExerciseCommand(strength.Id, "Bench", 5000), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.ActivityTypeMismatch);
        }

        [Fact]
        public async Task Handle_DistanceUnchanged_RenamesAndSavesWithoutCheckingUsage()
        {
            var cardio = OwnedCardio(distance: 5000);
            _exercises.GetByIdAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(cardio);

            var result = await CreateHandler().Handle(
                new UpdateCardioExerciseCommand(cardio.Id, "Evening run", 5000), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            cardio.Name.ShouldBe("Evening run");
            cardio.Distance.ShouldBe(5000);
            // The in-use rule guards the locked field — it must not run when distance is unchanged.
            await _workouts.DidNotReceive().ExistsByExerciseAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_DistanceChangedWhileExerciseInUse_ReturnsInUseAndDoesNotSave()
        {
            var cardio = OwnedCardio(distance: 5000);
            _exercises.GetByIdAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(cardio);
            _workouts.ExistsByExerciseAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(true);

            var result = await CreateHandler().Handle(
                new UpdateCardioExerciseCommand(cardio.Id, "Run", 6000), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.InUse);
            cardio.Distance.ShouldBe(5000); // distance change rejected
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_DistanceChangedWhileExerciseNotInUse_ChangesDistanceAndSaves()
        {
            var cardio = OwnedCardio(distance: 5000);
            _exercises.GetByIdAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(cardio);
            _workouts.ExistsByExerciseAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(false);

            var result = await CreateHandler().Handle(
                new UpdateCardioExerciseCommand(cardio.Id, "Run", 6000), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            cardio.Distance.ShouldBe(6000);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
