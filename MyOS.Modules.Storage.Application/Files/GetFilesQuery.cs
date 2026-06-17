using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Files.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Storage.Application.Files
{
    public sealed record GetFilesQuery : IQuery<IReadOnlyList<StoredFileDto>>;

    internal sealed class GetFilesQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetFilesQuery, IReadOnlyList<StoredFileDto>>
    {
        public async Task<Result<IReadOnlyList<StoredFileDto>>> Handle(
            GetFilesQuery query, CancellationToken cancellationToken)
        {
            var files = await db.Query("storage.v_files")
                .Where("user_id", currentUser.Id)
                .OrderByDesc("created_at_utc")
                .GetAsync<StoredFileDto>(cancellationToken: cancellationToken);

            return Result<IReadOnlyList<StoredFileDto>>.Success(files.ToList());
        }
    }
}
