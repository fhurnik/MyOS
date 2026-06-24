using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Folders;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Folders
{
    public class MoveFolderCommandHandlerTests
    {
        private readonly IFolderRepository _folders = Substitute.For<IFolderRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private MoveFolderCommandHandler CreateHandler() =>
            new(_folders, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_FolderDoesNotExist_ReturnsNotFound()
        {
            _folders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Folder?)null);

            var result = await CreateHandler().Handle(
                new MoveFolderCommand(Guid.NewGuid(), null), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.NotFound);
        }

        [Fact]
        public async Task Handle_FolderOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersFolder = Folder.Create(Guid.NewGuid(), "f", null);
            _folders.GetByIdAsync(othersFolder.Id, Arg.Any<CancellationToken>()).Returns(othersFolder);

            var result = await CreateHandler().Handle(
                new MoveFolderCommand(othersFolder.Id, null), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_TargetParentMissingOrNotOwned_ReturnsParentNotFound()
        {
            var folder = Folder.Create(_currentUser.Id, "f", null);
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
            // any other id resolves to null (the requested parent does not exist)
            _folders.GetByIdAsync(Arg.Is<Guid>(id => id != folder.Id), Arg.Any<CancellationToken>())
                .Returns((Folder?)null);

            var result = await CreateHandler().Handle(
                new MoveFolderCommand(folder.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.ParentNotFound);
        }

        [Fact]
        public async Task Handle_TargetIsInsideOwnSubtree_ReturnsCircularReference()
        {
            var folder = Folder.Create(_currentUser.Id, "F", null);
            var child = Folder.Create(_currentUser.Id, "C", folder.Id);
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
            _folders.GetByIdAsync(child.Id, Arg.Any<CancellationToken>()).Returns(child);
            _folders.GetActiveSubtreeAsync(folder.Id, Arg.Any<CancellationToken>())
                .Returns([folder, child]);

            // Moving F under its own child C would create a cycle.
            var result = await CreateHandler().Handle(
                new MoveFolderCommand(folder.Id, child.Id), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.CircularReference);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_MoveToRoot_UpdatesParentAndSavesOnce()
        {
            var folder = Folder.Create(_currentUser.Id, "F", Guid.NewGuid());
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
            _folders.GetActiveSubtreeAsync(folder.Id, Arg.Any<CancellationToken>()).Returns([folder]);

            var result = await CreateHandler().Handle(
                new MoveFolderCommand(folder.Id, null), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            folder.ParentId.ShouldBeNull();
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
