using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Application.Files
{
    public sealed record DeleteFileCommand(Guid Id) : ICommand<Unit>;

    internal sealed class DeleteFileCommandHandler(
        IStoredFileRepository fileRepository,
        IStorageQuotaRepository quotaRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<DeleteFileCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
        {
            var file = await fileRepository.GetByIdAsync(command.Id, cancellationToken);

            if (file is null)
                return Result<Unit>.Failure(FileErrors.NotFound);

            if (file.UserId != currentUser.Id)
                return Result<Unit>.Failure(FileErrors.Forbidden);

            // Soft delete keeps the physical file in place (cleanup service purges it after retention);
            // quota is freed immediately so the user regains space at once.
            var quota = await quotaRepository.GetByUserIdAsync(currentUser.Id, cancellationToken);

            file.Delete();
            quota?.Release(file.SizeBytes);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
