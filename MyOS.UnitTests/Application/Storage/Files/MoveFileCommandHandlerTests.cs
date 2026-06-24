using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Files
{
    public class MoveFileCommandHandlerTests
    {
        private readonly IStoredFileRepository _files = Substitute.For<IStoredFileRepository>();
        private readonly IFolderRepository _folders = Substitute.For<IFolderRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private MoveFileCommandHandler CreateHandler() =>
            new(_files, _folders, _currentUser, _unitOfWork);

        private StoredFile OwnedFile() =>
            StoredFile.Create(_currentUser.Id, null, "a.png", "png", "image/png", 100);

        [Fact]
        public async Task Handle_FileDoesNotExist_ReturnsNotFound()
        {
            _files.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((StoredFile?)null);

            var result = await CreateHandler().Handle(
                new MoveFileCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(FileErrors.NotFound);
        }

        [Fact]
        public async Task Handle_FileOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersFile = StoredFile.Create(Guid.NewGuid(), null, "a.png", "png", "image/png", 100);
            _files.GetByIdAsync(othersFile.Id, Arg.Any<CancellationToken>()).Returns(othersFile);

            var result = await CreateHandler().Handle(
                new MoveFileCommand(othersFile.Id, null), CancellationToken.None);

            result.Error.ShouldBe(FileErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_TargetFolderMissingOrNotOwned_ReturnsParentNotFound()
        {
            var file = OwnedFile();
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
            _folders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Folder?)null);

            var result = await CreateHandler().Handle(
                new MoveFileCommand(file.Id, Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.ParentNotFound);
        }

        [Fact]
        public async Task Handle_MoveToOwnedFolder_UpdatesFolderAndSavesOnce()
        {
            var file = OwnedFile();
            var folder = Folder.Create(_currentUser.Id, "dest", null);
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

            var result = await CreateHandler().Handle(
                new MoveFileCommand(file.Id, folder.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            file.FolderId.ShouldBe(folder.Id);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_MoveToRoot_SkipsFolderCheckAndSaves()
        {
            var file = OwnedFile();
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);

            var result = await CreateHandler().Handle(
                new MoveFileCommand(file.Id, null), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            file.FolderId.ShouldBeNull();
            await _folders.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        }
    }
}
