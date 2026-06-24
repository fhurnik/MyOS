using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Notes.Application.Errors;
using MyOS.Modules.Notes.Application.Notes.CheckList;
using MyOS.Modules.Notes.Domain.Notes.CheckList;
using MyOS.UnitTests.Common;
using DomainCheckList = MyOS.Modules.Notes.Domain.Notes.CheckList.CheckList;

namespace MyOS.UnitTests.Application.Notes.CheckList
{
    public class UpdateCheckListItemCommandHandlerTests
    {
        private readonly ICheckListRepository _checkLists = Substitute.For<ICheckListRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private UpdateCheckListItemCommandHandler CreateHandler() =>
            new(_checkLists, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_ListDoesNotExist_ReturnsNotFoundAndDoesNotSave()
        {
            _checkLists.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DomainCheckList?)null);

            var result = await CreateHandler().Handle(
                new UpdateCheckListItemCommand(Guid.NewGuid(), Guid.NewGuid(), "new"), CancellationToken.None);

            result.Error.ShouldBe(CheckListErrors.NotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ListOwnedByAnotherUser_ReturnsForbiddenAndDoesNotSave()
        {
            var othersList = DomainCheckList.Create(Guid.NewGuid(), "L");
            var item = othersList.AddItem("milk");
            _checkLists.GetByIdAsync(othersList.Id, Arg.Any<CancellationToken>()).Returns(othersList);

            var result = await CreateHandler().Handle(
                new UpdateCheckListItemCommand(othersList.Id, item.Id, "new"), CancellationToken.None);

            result.Error.ShouldBe(CheckListErrors.Forbidden);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ItemNotInList_ReturnsItemNotFoundAndDoesNotSave()
        {
            var list = DomainCheckList.Create(_currentUser.Id, "L");
            _checkLists.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);

            var result = await CreateHandler().Handle(
                new UpdateCheckListItemCommand(list.Id, Guid.NewGuid(), "new"), CancellationToken.None);

            result.Error.ShouldBe(CheckListErrors.ItemNotFound);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_OwnedListWithItem_UpdatesTextAndSavesOnce()
        {
            var list = DomainCheckList.Create(_currentUser.Id, "L");
            var item = list.AddItem("milk");
            _checkLists.GetByIdAsync(list.Id, Arg.Any<CancellationToken>()).Returns(list);

            var result = await CreateHandler().Handle(
                new UpdateCheckListItemCommand(list.Id, item.Id, "almond milk"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            item.Text.ShouldBe("almond milk");
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
