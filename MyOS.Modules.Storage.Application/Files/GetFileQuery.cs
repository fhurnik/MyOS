using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Files.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Storage.Application.Files
{
    public sealed record GetFileQuery(Guid Id) : IQuery<StoredFileDto>;

    internal sealed class GetFileQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetFileQuery, StoredFileDto>
    {
        public async Task<Result<StoredFileDto>> Handle(GetFileQuery query, CancellationToken cancellationToken)
        {
            var file = await db.Query("storage.v_files")
                .Where("id", query.Id)
                .Where("user_id", currentUser.Id)
                .FirstOrDefaultAsync<StoredFileDto>(cancellationToken: cancellationToken);

            if (file is null)
                return Result<StoredFileDto>.Failure(FileErrors.NotFound);

            return Result<StoredFileDto>.Success(file);
        }
    }
}
