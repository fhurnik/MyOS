using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Folders;

namespace MyOS.Modules.Storage.Application.Files
{
    public sealed record MoveFileCommand(Guid FileId, Guid? FolderId) : ICommand<Unit>;

    internal sealed class MoveFileCommandHandler(
        IStoredFileRepository fileRepository,
        IFolderRepository folderRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<MoveFileCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(MoveFileCommand command, CancellationToken cancellationToken)
        {
            var file = await fileRepository.GetByIdAsync(command.FileId, cancellationToken);

            if (file is null)
                return Result<Unit>.Failure(FileErrors.NotFound);

            if (file.UserId != currentUser.Id)
                return Result<Unit>.Failure(FileErrors.Forbidden);

            if (command.FolderId is not null)
            {
                var folder = await folderRepository.GetByIdAsync(command.FolderId.Value, cancellationToken);
                if (folder is null || folder.UserId != currentUser.Id)
                    return Result<Unit>.Failure(FolderErrors.ParentNotFound);
            }

            file.MoveTo(command.FolderId);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
