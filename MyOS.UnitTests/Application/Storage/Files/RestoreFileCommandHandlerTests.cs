using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.Modules.Storage.Domain.Quotas;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Files
{
    public class RestoreFileCommandHandlerTests
    {
        private readonly IStoredFileRepository _files = Substitute.For<IStoredFileRepository>();
        private readonly IFolderRepository _folders = Substitute.For<IFolderRepository>();
        private readonly IStorageQuotaRepository _quotas = Substitute.For<IStorageQuotaRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private RestoreFileCommandHandler CreateHandler() =>
            new(_files, _folders, _quotas, _currentUser, _unitOfWork);

        private StoredFile DeletedFile(Guid? folderId = null, long size = 100)
        {
            var file = StoredFile.Create(_currentUser.Id, folderId, "a.png", "png", "image/png", size);
            file.Delete();
            return file;
        }

        [Fact]
        public async Task Handle_DeletedFileDoesNotExist_ReturnsNotFound()
        {
            _files.GetDeletedByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((StoredFile?)null);

            var result = await CreateHandler().Handle(new RestoreFileCommand(Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(FileErrors.NotFound);
        }

        [Fact]
        public async Task Handle_FileOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersFile = StoredFile.Create(Guid.NewGuid(), null, "a.png", "png", "image/png", 100);
            othersFile.Delete();
            _files.GetDeletedByIdAsync(othersFile.Id, Arg.Any<CancellationToken>()).Returns(othersFile);

            var result = await CreateHandler().Handle(new RestoreFileCommand(othersFile.Id), CancellationToken.None);

            result.Error.ShouldBe(FileErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_QuotaCannotFitRestoredFile_ReturnsInsufficientSpace()
        {
            var file = DeletedFile(size: 100);
            _files.GetDeletedByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>())
                .Returns(StorageQuota.Create(_currentUser.Id, maxBytes: 2));

            var result = await CreateHandler().Handle(new RestoreFileCommand(file.Id), CancellationToken.None);

            result.Error.ShouldBe(QuotaErrors.InsufficientSpace);
        }

        [Fact]
        public async Task Handle_FolderStillExists_RestoresKeepsFolderConsumesQuotaAndSaves()
        {
            var folder = Folder.Create(_currentUser.Id, "dest", null);
            var file = DeletedFile(folderId: folder.Id, size: 100);
            var quota = StorageQuota.Create(_currentUser.Id);
            _files.GetDeletedByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>()).Returns(quota);
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);

            var result = await CreateHandler().Handle(new RestoreFileCommand(file.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            file.DeletedAtUtc.ShouldBeNull();
            file.FolderId.ShouldBe(folder.Id);
            quota.UsedBytes.ShouldBe(100);
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_OriginalFolderGone_RestoresToRoot()
        {
            var file = DeletedFile(folderId: Guid.NewGuid(), size: 100);
            _files.GetDeletedByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>())
                .Returns(StorageQuota.Create(_currentUser.Id));
            _folders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Folder?)null);

            var result = await CreateHandler().Handle(new RestoreFileCommand(file.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            file.FolderId.ShouldBeNull(); // moved to root because its folder no longer exists
        }
    }
}
