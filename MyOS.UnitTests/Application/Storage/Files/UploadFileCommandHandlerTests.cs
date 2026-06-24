using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files;
using MyOS.Modules.Storage.Domain.AllowedFileTypes;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.Modules.Storage.Domain.Quotas;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Files
{
    public class UploadFileCommandHandlerTests
    {
        private readonly IStoredFileRepository _files = Substitute.For<IStoredFileRepository>();
        private readonly IStorageQuotaRepository _quotas = Substitute.For<IStorageQuotaRepository>();
        private readonly IAllowedFileTypeRepository _allowedTypes = Substitute.For<IAllowedFileTypeRepository>();
        private readonly IFolderRepository _folders = Substitute.For<IFolderRepository>();
        private readonly IFileStorage _fileStorage = Substitute.For<IFileStorage>();
        private readonly IFileSignatureValidator _signature = Substitute.For<IFileSignatureValidator>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private UploadFileCommandHandler CreateHandler() =>
            new(_files, _quotas, _allowedTypes, _folders, _fileStorage, _signature, _currentUser, _unitOfWork);

        private static UploadFileCommand Command(long size = 3, Guid? folderId = null) =>
            new(new MemoryStream([1, 2, 3]), "photo.png", "image/png", size, folderId);

        [Fact]
        public async Task Handle_FolderProvidedButMissing_ReturnsParentNotFound()
        {
            _folders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Folder?)null);

            var result = await CreateHandler().Handle(Command(folderId: Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.ParentNotFound);
        }

        [Fact]
        public async Task Handle_FileTypeNotAllowed_ReturnsTypeNotAllowed()
        {
            _allowedTypes.GetByExtensionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns((AllowedFileType?)null);
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>())
                .Returns(StorageQuota.Create(_currentUser.Id));

            var result = await CreateHandler().Handle(Command(), CancellationToken.None);

            result.Error.ShouldBe(FileErrors.TypeNotAllowed);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_InsufficientQuota_ReturnsInsufficientSpace()
        {
            _allowedTypes.GetByExtensionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(StorageTestData.AllowedType());
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>())
                .Returns(StorageQuota.Create(_currentUser.Id, maxBytes: 2));

            var result = await CreateHandler().Handle(Command(size: 100), CancellationToken.None);

            result.Error.ShouldBe(QuotaErrors.InsufficientSpace);
        }

        [Fact]
        public async Task Handle_ContentSignatureInvalid_ReturnsContentMismatchAndDoesNotSave()
        {
            _allowedTypes.GetByExtensionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(StorageTestData.AllowedType());
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>())
                .Returns(StorageQuota.Create(_currentUser.Id));
            _signature.IsValidAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(false);

            var result = await CreateHandler().Handle(Command(), CancellationToken.None);

            result.Error.ShouldBe(FileErrors.ContentMismatch);
            await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ValidUpload_StoresBytesConsumesQuotaAndSavesOnce()
        {
            var quota = StorageQuota.Create(_currentUser.Id);
            _allowedTypes.GetByExtensionAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(StorageTestData.AllowedType(contentType: "image/png"));
            _quotas.GetByUserIdAsync(_currentUser.Id, Arg.Any<CancellationToken>()).Returns(quota);
            _signature.IsValidAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(true);

            var result = await CreateHandler().Handle(Command(size: 3), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            quota.UsedBytes.ShouldBe(3);
            await _fileStorage.Received(1).SaveAsync(
                _currentUser.Id, Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<CancellationToken>());
            await _files.Received(1).AddAsync(Arg.Any<StoredFile>(), Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
