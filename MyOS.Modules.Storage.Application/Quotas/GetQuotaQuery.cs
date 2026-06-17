using MyOS.Core.Application.Abstractions;
using MyOS.Core.Application.Abstractions.Messaging;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Modules.Storage.Application.Errors;
using MyOS.Modules.Storage.Application.Quotas.Shared;
using SqlKata.Execution;

namespace MyOS.Modules.Storage.Application.Quotas
{
    public sealed record GetQuotaQuery : IQuery<QuotaDto>;

    internal sealed class GetQuotaQueryHandler(
        QueryFactory db,
        ICurrentUser currentUser) : IQueryHandler<GetQuotaQuery, QuotaDto>
    {
        public async Task<Result<QuotaDto>> Handle(GetQuotaQuery query, CancellationToken cancellationToken)
        {
            var quota = await db.Query("storage.v_storage_quotas")
                .Where("user_id", currentUser.Id)
                .FirstOrDefaultAsync<QuotaDto>(cancellationToken: cancellationToken);

            if (quota is null)
                return Result<QuotaDto>.Failure(QuotaErrors.NotFound);

            return Result<QuotaDto>.Success(quota);
        }
    }
}
