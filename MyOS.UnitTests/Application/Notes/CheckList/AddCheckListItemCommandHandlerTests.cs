using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Application.Notes.CheckList;
using MyOS.Modules.Notes.Domain.Notes.CheckList;
using MyOS.UnitTests.Common;
using DomainCheckList = MyOS.Modules.Notes.Domain.Notes.CheckList.CheckList;

namespace MyOS.UnitTests.Application.Notes.CheckList
{
    public class AddCheckListItemCommandHandlerTests
    {
        private readonly ICheckListRepository _checkLists = Substitute.For<ICheckListRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private AddCheckListItemCommandHandler CreateHandler() =>
            new(_checkLists, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_ListDoesNotExist_ReturnsNotFoundAndDoesNotSave()
        {
            _checkLists.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DomainCheckList?)null);

            var result = await CreateHandler().Handle(
                new AddCheckListItemCommand(Guid.NewGuid(), "milk"), CancellationToken.None);

            result.Error.ShouldBe(CheckListErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ListOwnedByAnotherUser_ReturnsForbiddenAndDoesNotSave()
        {
            var othersList = DomainCheckList.Create(Guid.NewGuid(), "L");
            _checkLists.GetByIdAsync(othersList.Id, Arg.Any<CancellationToken>()).Returns(othersList);

            var result = await CreateHandler().Handle(
                new AddCheckListItemCommand(othersList.Id, "milk"), CancellationToken.None);

            result.Error.ShouldBe(CheckListErrors.Forbidden);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_OwnedList_AddsItemAndSavesOnce()
        {
            var list = DomainCheckList.Create(_currentUser.Id, "L");
            _checkLists.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);

            var result = await CreateHandler().Handle(
                new AddCheckListItemCommand(list.Id, "milk"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            list.Items.Count.ShouldBe(1);
            result.Value.ShouldBe(list.Items.Single().Id);
            await _checkLists.Received(1).AddItemAsync(Arg.Any<CheckListItem>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
