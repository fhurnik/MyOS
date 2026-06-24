using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Fitness.Application.Workouts;
using MyOS.Modules.Fitness.Domain.Workouts;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Fitness.Workouts
{
    public class CreateWorkoutCommandHandlerTests
    {
        private readonly IWorkoutRepository _workouts = Substitute.For<IWorkoutRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private CreateWorkoutCommandHandler CreateHandler() =>
            new(_workouts, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_ValidCommand_PersistsWorkoutForCurrentUserAndSavesOnce()
        {
            var date = new DateOnly(2026, 6, 21);

            var result = await CreateHandler().Handle(
                new CreateWorkoutCommand(date, "Leg day"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            await _workouts.Received(1).AddAsync(
                Arg.Is<Workout>(w => w.UserId == _currentUser.Id && w.Date == date && w.Notes == "Leg day"),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
