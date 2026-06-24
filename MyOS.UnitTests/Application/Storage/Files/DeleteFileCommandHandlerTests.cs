using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Quotas;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Files
{
    public class DeleteFileCommandHandlerTests
    {
        private readonly IStoredFileRepository _files = Substitute.For<IStoredFileRepository>();
        private readonly IStorageQuotaRepository _quotas = Substitute.For<IStorageQuotaRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private DeleteFileCommandHandler CreateHandler() =>
            new(_files, _quotas, _currentUser, _unitOfWork);

        private StoredFile OwnedFile(long size = 100) =>
            StoredFile.Create(_currentUser.Id, null, "a.png", "png", "image/png", size);

        [Fact]
        public async Task Handle_FileDoesNotExist_ReturnsNotFound()
        {
            _files.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((StoredFile?)null);

            var result = await CreateHandler().Handle(new DeleteFileCommand(Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(FileErrors.NotFound);
        }

        [Fact]
        public async Task Handle_FileOwnedByAnotherUser_ReturnsForbidden()
        {
            var othersFile = StoredFile.Create(Guid.NewGuid(), null, "a.png", "png", "image/png", 100);
            _files.GetByIdAsync(othersFile.Id, Arg.Any<CancellationToken>()).Returns(othersFile);

            var result = await CreateHandler().Handle(new DeleteFileCommand(othersFile.Id), CancellationToken.None);

            result.Error.ShouldBe(FileErrors.Forbidden);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_OwnedFile_SoftDeletesReleasesQuotaAndSavesOnce()
        {
            var file = OwnedFile(size: 100);
            var quota = StorageQuota.Create(_currentUser.Id);
            quota.Consume(500);
            _files.GetByIdAsync(file.Id, Arg.Any<CancellationToken>()).Returns(file);
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>()).Returns(quota);

            var result = await CreateHandler().Handle(new DeleteFileCommand(file.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            file.DeletedAtUtc.ShouldNotBeNull();
            quota.UsedBytes.ShouldBe(400); // 500 - 100 freed immediately
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
