using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Errors;
using MyOS.Modules.Fitness.Application.Targets;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.Modules.Fitness.Domain.Targets;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Targets
{
    public class SetExerciseTargetCommandHandlerTests
    {
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IExerciseTargetRepository _targets = Substitute.For<IExerciseTargetRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private SetExerciseTargetCommandHandler CreateHandler() =>
            new(_exercises, _targets, _currentUser, _unitOfWork);

        private StrengthExercise OwnedExercise() =>
            StrengthExercise.Create(_currentUser.Id, "Bench", StrengthCategory.Weighted);

        [Fact]
        public async Task Handle_ExerciseDoesNotExist_ReturnsNotFound()
        {
            _exercises.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns((Exercise?)null);

            var result = await CreateHandler().Handle(
                new SetExerciseTargetCommand(Guid.NewGuid(), 100m), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.NotFound);
        }

        [Fact]
        public async Task Handle_ExerciseOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersExercise = StrengthExercise.Create(Guid.NewGuid(), "Bench", StrengthCategory.Weighted);
            _exercises.GetByIdAsync(othersExercise.Id, Arg.Any<CancellationToken>()).Returns(othersExercise);

            var result = await CreateHandler().Handle(
                new SetExerciseTargetCommand(othersExercise.Id, 100m), CancellationToken.None);

            result.Error.ShouldBe(ExerciseErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_NoExistingTarget_CreatesTargetAndSavesOnce()
        {
            var exercise = OwnedExercise();
            _exercises.GetByIdAsync(exercise.Id, Arg.Any<CancellationToken>()).Returns(exercise);
            _targets.GetByExerciseIdAsync(exercise.Id, Arg.Any<CancellationToken>())
                .Returns((ExerciseTarget?)null);

            var result = await CreateHandler().Handle(
                new SetExerciseTargetCommand(exercise.Id, 120m), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            await _targets.Received(1).AddAsync(
                Arg.Is<ExerciseTarget>(t =>
                    t.ExerciseId == exercise.Id && t.UserId == _currentUser.Id && t.Value == 120m),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ExistingTarget_UpdatesValueAndDoesNotAddAgain()
        {
            var exercise = OwnedExercise();
            var existing = ExerciseTarget.Create(exercise.Id, _currentUser.Id, 100m);
            _exercises.GetByIdAsync(exercise.Id, Arg.Any<CancellationToken>()).Returns(exercise);
            _targets.GetByExerciseIdAsync(exercise.Id, Arg.Any<CancellationToken>()).Returns(existing);

            var result = await CreateHandler().Handle(
                new SetExerciseTargetCommand(exercise.Id, 130m), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            existing.Value.ShouldBe(130m);
            await _targets.DidNotReceive().AddAsync(Arg.Any<ExerciseTarget>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
