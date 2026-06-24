using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Exercises;
using MyOS.Modules.Fitness.Domain.Exercises;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Exercises
{
    public class CreateCardioExerciseCommandHandlerTests
    {
        private readonly IExerciseRepository _exercises = Substitute.For<IExerciseRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private CreateCardioExerciseCommandHandler CreateHandler() =>
            new(_exercises, _currentUser, _unitOfWork);

        // Create handlers have no failure branches — only the happy path. The value is asserting
        // the entity is built from the command + current user, and that it is persisted once.
        [Fact]
        public async Task Handle_ValidCommand_PersistsCardioExerciseForCurrentUserAndSavesOnce()
        {
            var result = await CreateHandler().Handle(
                new CreateCardioExerciseCommand("Morning run", Distance: 5000), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            await _exercises.Received(1).AddAsync(
                Arg.Is<CardioExercise>(e =>
                    e.UserId == _currentUser.Id && e.Name == "Morning run" && e.Distance == 5000),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
