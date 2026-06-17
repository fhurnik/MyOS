using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Folders;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Application.Folders
{
    public sealed record DeleteFolderCommand(Guid Id) : ICommand<Unit>;

    internal sealed class DeleteFolderCommandHandler(
        IFolderRepository folderRepository,
        IStoredFileRepository fileRepository,
        IStorageQuotaRepository quotaRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<DeleteFolderCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(DeleteFolderCommand command, CancellationToken cancellationToken)
        {
            var folder = await folderRepository.GetByIdAsync(command.Id, cancellationToken);

            if (folder is null)
                return Result<Unit>.Failure(FolderErrors.NotFound);

            if (folder.UserId != currentUser.Id)
                return Result<Unit>.Failure(FolderErrors.Forbidden);

            // Cascade: soft-delete the folder, all of its descendants, and the files within them.
            // Quota is freed at once for every removed file (same decision as single-file delete).
            var subtree = await folderRepository.GetActiveSubtreeAsync(folder.Id, cancellationToken);
            var folderIds = subtree.Select(f => f.Id).ToList();

            var files = await fileRepository.GetActiveByFolderIdsAsync(folderIds, cancellationToken);
            var quota = await quotaRepository.GetByUserIdAsync(currentUser.Id, cancellationToken);

            long releasedBytes = 0;
            foreach (var file in files)
            {
                file.Delete();
                releasedBytes += file.SizeBytes;
            }

            foreach (var subfolder in subtree)
                subfolder.Delete();

            quota?.Release(releasedBytes);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
