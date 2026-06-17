using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Abstractions;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Storage.Application.Files
{
    public sealed record GetFileForDownloadQuery(Guid Id, bool Inline) : IQuery<FileDownloadDto>;

    internal sealed class GetFileForDownloadQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser,
        IFileStorage fileStorage) : IQueryHandler<GetFileForDownloadQuery, FileDownloadDto>
    {
        public async Task<Result<FileDownloadDto>> Handle(
            GetFileForDownloadQuery query, CancellationToken cancellationToken)
        {
            var row = await db.Query("storage.v_files")
                .Select("storage_file_name", "original_name", "content_type")
                .Where("id", query.Id)
                .Where("user_id", currentUser.Id)
                .FirstOrDefaultAsync<FileLocation>(cancellationToken: cancellationToken);

            if (row is null)
                return Result<FileDownloadDto>.Failure(FileErrors.NotFound);

            // Inline serving is only for media that is actually played in the browser; everything
            // else (pdf, documents, archives, text) must go through the attachment download.
            if (query.Inline && !IsInlineViewable(row.ContentType))
                return Result<FileDownloadDto>.Failure(FileErrors.InlineNotAllowed);

            if (!fileStorage.Exists(currentUser.Id, row.StorageFileName))
                return Result<FileDownloadDto>.Failure(FileErrors.PhysicalFileMissing);

            return Result<FileDownloadDto>.Success(new FileDownloadDto
            {
                AbsolutePath = fileStorage.GetAbsolutePath(currentUser.Id, row.StorageFileName),
                ContentType = row.ContentType,
                OriginalName = row.OriginalName
            });
        }

        private static bool IsInlineViewable(string contentType) =>
            contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase) ||
            contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);

        private sealed record FileLocation
        {
            public string StorageFileName { get; init; } = string.Empty;
            public string OriginalName { get; init; } = string.Empty;
            public string ContentType { get; init; } = string.Empty;
        }
    }
}
