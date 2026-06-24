using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Exercises;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Exercises
{
    public class UpdateStrengthExerciseCommandHandlerTests
    {
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private UpdateStrengthExerciseCommandHandler CreateHandler() =>
            new(_exercises, _workouts, _currentUser, _unitOfWork);

        private StrengthExercise OwnedStrength(StrengthCategory category = StrengthCategory.Weighted) =>
            StrengthExercise.Create(_currentUser.Id, "Bench", category);

        [Fact]
        public async Task Handle_ExerciseDoesNotExist_ReturnsNotFound()
        {
            _exercises.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Exercise?)null);

            var result = await CreateHandler().Handle(
                new UpdateStrengthExerciseCommand(Guid.NewGuid(), "Bench", StrengthCategory.Weighted),
                CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.NotFound);
        }

        [Fact]
        public async Task Handle_ExerciseOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersStrength = StrengthExercise.Create(Guid.NewGuid(), "Bench", StrengthCategory.Weighted);
            _exercises.GetByIdAsync(othersStrength.Id, Arg.Any<CancellationToken>()).Returns(othersStrength);

            var result = await CreateHandler().Handle(
                new UpdateStrengthExerciseCommand(othersStrength.Id, "Bench", StrengthCategory.Weighted),
                CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_ExerciseIsNotStrength_ReturnsActivityTypeMismatch()
        {
            var cardio = CardioExercise.Create(_currentUser.Id, "Run", 5000);
            _exercises.GetByIdAsync(cardio.Id, Arg.Any<CancellationToken>()).Returns(cardio);

            var result = await CreateHandler().Handle(
                new UpdateStrengthExerciseCommand(cardio.Id, "Run", StrengthCategory.Weighted),
                CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.ActivityTypeMismatch);
        }

        [Fact]
        public async Task Handle_CategoryUnchanged_RenamesAndSavesWithoutCheckingUsage()
        {
            var strength = OwnedStrength(StrengthCategory.Weighted);
            _exercises.GetByIdAsync(strength.Id, Arg.Any<CancellationToken>()).Returns(strength);

            var result = await CreateHandler().Handle(
                new UpdateStrengthExerciseCommand(strength.Id, "Incline bench", StrengthCategory.Weighted),
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            strength.Name.ShouldBe("Incline bench");
            strength.StrengthCategory.ShouldBe(StrengthCategory.Weighted);
            await _workouts.DidNotReceive().ExistsByExerciseAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_CategoryChangedWhileExerciseInUse_ReturnsInUseAndDoesNotSave()
        {
            var strength = OwnedStrength(StrengthCategory.Weighted);
            _exercises.GetByIdAsync(strength.Id, Arg.Any<CancellationToken>()).Returns(strength);
            _workouts.ExistsByExerciseAsync(strength.Id, Arg.Any<CancellationToken>()).Returns(true);

            var result = await CreateHandler().Handle(
                new UpdateStrengthExerciseCommand(strength.Id, "Bench", StrengthCategory.Bodyweight),
                CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.InUse);
            strength.StrengthCategory.ShouldBe(StrengthCategory.Weighted); // category change rejected
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_CategoryChangedWhileExerciseNotInUse_ChangesCategoryAndSaves()
        {
            var strength = OwnedStrength(StrengthCategory.Weighted);
            _exercises.GetByIdAsync(strength.Id, Arg.Any<CancellationToken>()).Returns(strength);
            _workouts.ExistsByExerciseAsync(strength.Id, Arg.Any<CancellationToken>()).Returns(false);

            var result = await CreateHandler().Handle(
                new UpdateStrengthExerciseCommand(strength.Id, "Bench", StrengthCategory.Bodyweight),
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            strength.StrengthCategory.ShouldBe(StrengthCategory.Bodyweight);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
