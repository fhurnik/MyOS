using FluentValidation;
using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Domain.Folders;

namespace MyOS.Modules.Storage.Application.Folders
{
    public sealed record RenameFolderCommand(Guid Id, string Name) : ICommand<Unit>;

    public sealed class RenameFolderCommandValidator : AbstractValidator<RenameFolderCommand>
    {
        public RenameFolderCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        }
    }

    internal sealed class RenameFolderCommandHandler(
        IFolderRepository folderRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<RenameFolderCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(RenameFolderCommand command, CancellationToken cancellationToken)
        {
            var folder = await folderRepository.GetByIdAsync(command.Id, cancellationToken);

            if (folder is null)
                return Result<Unit>.Failure(FolderErrors.NotFound);

            if (folder.UserId != currentUser.Id)
                return Result<Unit>.Failure(FolderErrors.Forbidden);

            folder.Rename(command.Name);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
