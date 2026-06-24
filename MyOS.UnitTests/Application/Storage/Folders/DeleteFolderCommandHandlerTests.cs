using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Folders;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.Modules.Storage.Domain.Quotas;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Folders
{
    public class DeleteFolderCommandHandlerTests
    {
        private readonly IFolderRepository _folders = Substitute.For<IFolderRepository>();
        private readonly IStoredFileRepository _files = Substitute.For<IStoredFileRepository>();
        private readonly IStorageQuotaRepository _quotas = Substitute.For<IStorageQuotaRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private DeleteFolderCommandHandler CreateHandler() =>
            new(_folders, _files, _quotas, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_FolderDoesNotExist_ReturnsNotFound()
        {
            _folders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Folder?)null);

            var result = await CreateHandler().Handle(new DeleteFolderCommand(Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.NotFound);
        }

        [Fact]
        public async Task Handle_FolderOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersFolder = Folder.Create(Guid.NewGuid(), "f", null);
            _folders.GetByIdAsync(othersFolder.Id, Arg.Any<CancellationToken>()).Returns(othersFolder);

            var result = await CreateHandler().Handle(new DeleteFolderCommand(othersFolder.Id), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.Forbidden);
        }

        [Fact]
        public async Task Handle_OwnedFolder_CascadeSoftDeletesSubtreeAndFilesAndReleasesQuota()
        {
            var folder = Folder.Create(_currentUser.Id, "F", null);
            var file = StoredFile.Create(_currentUser.Id, folder.Id, "a.png", "png", "image/png", 100);
            var quota = StorageQuota.Create(_currentUser.Id);
            quota.Consume(500);
            _folders.GetByIdAsync(folder.Id, Arg.Any<CancellationToken>()).Returns(folder);
            _folders.GetActiveSubtreeAsync(folder.Id, Arg.Any<CancellationToken>()).Returns([folder]);
            _files.GetActiveByFolderIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
                .Returns([file]);
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>()).Returns(quota);

            var result = await CreateHandler().Handle(new DeleteFolderCommand(folder.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            folder.DeletedAtUtc.ShouldNotBeNull();
            file.DeletedAtUtc.ShouldNotBeNull();
            quota.UsedBytes.ShouldBe(400); // 500 - 100 freed
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
