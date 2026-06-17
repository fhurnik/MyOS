using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Folders.BusinesRules;
using MyOS.Modules.Storage.Domain.Folders;

namespace MyOS.Modules.Storage.Application.Folders
{
    public sealed record MoveFolderCommand(Guid Id, Guid? ParentId) : ICommand<Unit>;

    internal sealed class MoveFolderCommandHandler(
        IFolderRepository folderRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<MoveFolderCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(MoveFolderCommand command, CancellationToken cancellationToken)
        {
            var folder = await folderRepository.GetByIdAsync(command.Id, cancellationToken);

            if (folder is null)
                return Result<Unit>.Failure(FolderErrors.NotFound);

            if (folder.UserId != currentUser.Id)
                return Result<Unit>.Failure(FolderErrors.Forbidden);

            if (command.ParentId is not null)
            {
                var parent = await folderRepository.GetByIdAsync(command.ParentId.Value, cancellationToken);
                if (parent is null || parent.UserId != currentUser.Id)
                    return Result<Unit>.Failure(FolderErrors.ParentNotFound);
            }

            var subtree = await folderRepository.GetActiveSubtreeAsync(folder.Id, cancellationToken);
            var subtreeIds = subtree.Select(f => f.Id).ToList();

            var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
                new FolderMustNotCreateCycleRule(command.ParentId, subtreeIds));

            if (check.IsFailure)
                return Result<Unit>.Failure(check.Error);

            folder.MoveTo(command.ParentId);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
