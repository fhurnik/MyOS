using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Folders.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Storage.Application.Folders
{
    public sealed record GetFoldersQuery : IQuery<IReadOnlyList<FolderDto>>;

    internal sealed class GetFoldersQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetFoldersQuery, IReadOnlyList<FolderDto>>
    {
        public async Task<Result<IReadOnlyList<FolderDto>>> Handle(
            GetFoldersQuery query, CancellationToken cancellationToken)
        {
            var folders = await db.Query("storage.v_folders")
                .Where("user_id", currentUser.Id)
                .OrderBy("name")
                .GetAsync<FolderDto>(cancellationToken: cancellationToken);

            return Result<IReadOnlyList<FolderDto>>.Success(folders.ToList());
        }
    }
}
