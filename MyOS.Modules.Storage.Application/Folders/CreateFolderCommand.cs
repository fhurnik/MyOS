using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Domain.Folders;

namespace MyOS.Modules.Storage.Application.Folders
{
    public sealed record CreateFolderCommand(string Name, Guid? ParentId) : ICommand<Guid>;

    public sealed class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
    {
        public CreateFolderCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(255);
        }
    }

    internal sealed class CreateFolderCommandHandler(
        IFolderRepository folderRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<CreateFolderCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(CreateFolderCommand command, CancellationToken cancellationToken)
        {
            if (command.ParentId is not null)
            {
                var parent = await folderRepository.GetByIdAsync(command.ParentId.Value, cancellationToken);
                if (parent is null || parent.UserId != currentUser.Id)
                    return Result<Guid>.Failure(FolderErrors.ParentNotFound);
            }

            var folder = Folder.Create(currentUser.Id, command.Name, command.ParentId);

            await folderRepository.AddAsync(folder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(folder.Id);
        }
    }
}
