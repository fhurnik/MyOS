using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Files.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Storage.Application.Files
{
    /// <summary>Lists the current user's soft-deleted files (the "trash"), so they can be restored.</summary>
    public sealed record GetDeletedFilesQuery : IQuery<IReadOnlyList<StoredFileDto>>;

    internal sealed class GetDeletedFilesQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetDeletedFilesQuery, IReadOnlyList<StoredFileDto>>
    {
        public async Task<Result<IReadOnlyList<StoredFileDto>>> Handle(
            GetDeletedFilesQuery query, CancellationToken cancellationToken)
        {
            var files = await db.Query("storage.v_deleted_files")
                .Where("user_id", currentUser.Id)
                .OrderByDesc("deleted_at_utc")
                .GetAsync<StoredFileDto>(cancellationToken: cancellationToken);

            return Result<IReadOnlyList<StoredFileDto>>.Success(files.ToList());
        }
    }
}
