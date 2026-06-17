using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.AllowedFileTypes.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Storage.Application.AllowedFileTypes
{
    public sealed record GetAllowedFileTypesQuery : IQuery<IReadOnlyList<AllowedFileTypeDto>>;

    internal sealed class GetAllowedFileTypesQueryHandler(
        QueryFactory db) : IQueryHandler<GetAllowedFileTypesQuery, IReadOnlyList<AllowedFileTypeDto>>
    {
        public async Task<Result<IReadOnlyList<AllowedFileTypeDto>>> Handle(
            GetAllowedFileTypesQuery query, CancellationToken cancellationToken)
        {
            var types = await db.Query("storage.v_allowed_file_types")
                .OrderBy("category", "extension")
                .GetAsync<AllowedFileTypeDto>(cancellationToken: cancellationToken);

            return Result<IReadOnlyList<AllowedFileTypeDto>>.Success(types.ToList());
        }
    }
}
