using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Exercises;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Exercises
{
    public class CreateStrengthExerciseCommandHandlerTests
    {
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private CreateStrengthExerciseCommandHandler CreateHandler() =>
            new(_exercises, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_ValidCommand_PersistsStrengthExerciseForCurrentUserAndSavesOnce()
        {
            var result = await CreateHandler().Handle(
                new CreateStrengthExerciseCommand("Bench press", StrengthCategory.Weighted),
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            await _exercises.Received(1).AddAsync(
                Arg.Is<StrengthExercise>(e =>
                    e.UserId == _currentUser.Id && e.Name == "Bench press" &&
                    e.StrengthCategory == StrengthCategory.Weighted),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
