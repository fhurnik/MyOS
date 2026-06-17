using FluentValidation;
using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.BusinessRules;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files.BusinesRules;
using MyOS.Modules.Storage.Domain.AllowedFileTypes;
using MyOS.Modules.Storage.Domain.Files;
using MyOS.Modules.Storage.Domain.Quotas;

namespace MyOS.Modules.Storage.Application.Files
{
    public sealed record UploadFileCommand(
        Stream Content,
        string FileName,
        string ContentType,
        long SizeBytes) : ICommand<Guid>;

    public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
    {
        public UploadFileCommandValidator()
        {
            RuleFor(x => x.Content).NotNull();
            RuleFor(x => x.SizeBytes).GreaterThan(0);
            RuleFor(x => x.ContentType).NotEmpty();
            RuleFor(x => x.FileName)
                .NotEmpty()
                .MaximumLength(255)
                .Must(name => !string.IsNullOrEmpty(Path.GetExtension(name)))
                .WithMessage("File name must include an extension.");
        }
    }

    internal sealed class UploadFileCommandHandler(
        IStoredFileRepository fileRepository,
        IStorageQuotaRepository quotaRepository,
        IAllowedFileTypeRepository allowedFileTypeRepository,
        IFileStorage fileStorage,
        IFileSignatureValidator signatureValidator,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork) : ICommandHandler<UploadFileCommand, Guid>
    {
        public async Task<Result<Guid>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(command.FileName).TrimStart('.').ToLowerInvariant();

            var allowedType = await allowedFileTypeRepository.GetByExtensionAsync(extension, cancellationToken);
            var quota = await quotaRepository.GetByUserIdAsync(currentUser.Id, cancellationToken);

            var check = await BusinessRuleChecker.CheckAsync(cancellationToken,
                new FileTypeMustBeAllowedRule(allowedType),
                new QuotaMustHaveSpaceRule(quota, command.SizeBytes));

            if (check.IsFailure)
                return Result<Guid>.Failure(check.Error);

            if (command.Content.CanSeek)
                command.Content.Position = 0;

            if (!await signatureValidator.IsValidAsync(command.Content, extension, cancellationToken))
                return Result<Guid>.Failure(FileErrors.ContentMismatch);

            if (command.Content.CanSeek)
                command.Content.Position = 0;

            var file = StoredFile.Create(
                currentUser.Id, command.FileName, extension, allowedType!.ContentType, command.SizeBytes);

            // Two-store write: persist bytes first, then metadata + quota in one transaction.
            // If the DB write fails, compensate by removing the orphaned file from disk.
            await fileStorage.SaveAsync(currentUser.Id, file.StorageFileName, command.Content, cancellationToken);

            try
            {
                await fileRepository.AddAsync(file, cancellationToken);
                quota!.Consume(command.SizeBytes);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                fileStorage.Delete(currentUser.Id, file.StorageFileName);
                throw;
            }

            return Result<Guid>.Success(file.Id);
        }
    }
}
