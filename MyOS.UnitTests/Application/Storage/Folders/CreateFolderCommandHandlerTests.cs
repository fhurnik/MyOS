using MyOS.Core.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Folders;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.UnitTests.Common;

namespace MyOS.UnitTests.Application.Storage.Folders
{
    public class CreateFolderCommandHandlerTests
    {
        private readonly IFolderRepository _folders = Substitute.For<IFolderRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly FakeCurrentUser _currentUser = new();

        private CreateFolderCommandHandler CreateHandler() =>
            new(_folders, _currentUser, _unitOfWork);

        [Fact]
        public async Task Handle_ParentMissingOrNotOwned_ReturnsParentNotFound()
        {
            _folders.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Folder?)null);

            var result = await CreateHandler().Handle(
                new CreateFolderCommand("docs", Guid.NewGuid()), CancellationToken.None);

            result.Error.ShouldBe(FolderErrors.ParentNotFound);
        }

        [Fact]
        public async Task Handle_AtRoot_CreatesFolderForCurrentUserAndSavesOnce()
        {
            var result = await CreateHandler().Handle(
                new CreateFolderCommand("docs", null), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBe(Guid.Empty);
            await _folders.Received(1).AddAsync(
                Arg.Is<Folder>(f => f.UserId == _currentUser.Id && f.Name == "docs" && f.ParentId == null),
                Arg.Any<CancellationToken>());
            await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_UnderOwnedParent_CreatesNestedFolder()
        {
            var parent = Folder.Create(_currentUser.Id, "parent", null);
            _folders.GetByIdAsync(parent.Id, Arg.Any<CancellationToken>()).Returns(parent);

            var result = await CreateHandler().Handle(
                new CreateFolderCommand("child", parent.Id), CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            await _folders.Received(1).AddAsync(
                Arg.Is<Folder>(f => f.ParentId == parent.Id), Arg.Any<CancellationToken>());
        }
    }
}
