using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Folders;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Folders
{
    public class RenameFolderCommandHandlerTests
    {
        private readonly IFolderRepository _folders = Substitute.For<IFolderRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private RenameFolderCommandHandler CreateHandler() =>
            new(_folders, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_FolderDoesNotExist_ReturnsNotFound()
        {
            _folders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Folder?)null);

            var result = await CreateHandler().Handle(
                new RenameFolderCommand(Guid.NewGuid(), "new"), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.NotFound);
        }

        [Fact]
        public async Task Handle_FolderOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersFolder = Folder.Create(Guid.NewGuid(), "old", null);
            _folders.GetByIdAsync(othersFolder.Id, Arg.Any<CancellationToken>()).Returns(othersFolder);

            var result = await CreateHandler().Handle(
                new RenameFolderCommand(othersFolder.Id, "new"), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_OwnedFolder_RenamesAndSavesOnce()
        {
            var folder = Folder.Create(_currentUser.Id, "old", null);
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

            var result = await CreateHandler().Handle(
                new RenameFolderCommand(folder.Id, "new"), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            folder.Name.ShouldBe("new");
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
