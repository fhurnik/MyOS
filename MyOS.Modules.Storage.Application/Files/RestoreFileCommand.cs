using MediatR;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files.BusinesRules;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Application.Files
{
    public sealed record RestoreFileCommand(Guid Id) : ICommand<Unit>;

    internal sealed class RestoreFileCommandHandler(
        IStoredFileRepository fileRepository,
        IStorageQuotaRepository quotaRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<RestoreFileCommand, Unit>
    {
        public async Task<Result<Unit>> Handle(RestoreFileCommand command, CancellationToken cancellationToken)
        {
            var file = await fileRepository.GetDeletedByIdAsync(command.Id, cancellationToken);

            if (file is null)
                return Result<Unit>.Failure(FileErrors.NotFound);

            if (file.UserId != currentUser.Id)
                return Result<Unit>.Failure(FileErrors.Forbidden);

            var quota = await quotaRepository.GetByUserIdAsync(currentUser.Id, cancellationToken);

            // Restoring re-consumes space, so it must fit within the quota.
            var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
                new QuotaMustHaveSpaceRule(quota, file.SizeBytes));

            if (check.IsFailure)
                return Result<Unit>.Failure(check.Error);

            file.Restore();
            quota!.Consume(file.SizeBytes);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Unit>.Success(Unit.Value);
        }
    }
}
