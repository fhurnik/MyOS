using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Notes.Application.Notes.CheckList;
using MyOS.Modules.Notes.Domain.Notes.CheckList;
using MyOS.UnitTests.Common;
using DomainCheckList = MyOS.Modules.Notes.Domain.Notes.CheckList.CheckList;

namespace MyOS.UnitTests.Application.Notes.CheckList
{
    public class CreateCheckListCommandHandlerTests
    {
        private readonly ICheckListRepository _checkLists = Substitute.For<ICheckListRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private CreateCheckListCommandHandler CreateHandler() =>
            new(_checkLists, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_ValidCommand_PersistsListOwnedByCurrentUserAndSavesOnce()
        {
            DomainCheckList? added = null;
            await _checkLists.AddAsync(Arg.Do<DomainCheckList>(l => added = l), Arg.Any<CancellationToken>());

            var result = await CreateHandler().Handle(new CreateCheckListCommand("Groceries"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            added.ShouldNotBeNull();
            added!.UserId.ShouldBe(_currentUser.Id);
            result.Value.ShouldBe(added.Id);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
