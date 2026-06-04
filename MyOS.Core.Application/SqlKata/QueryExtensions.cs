using MyOS.Core.Application.Abstractions.Pagination;
using SqlKata;
using SqlKata.Execution;

namespace MyOS.Core.Application.SqlKata;

public static class QueryExtensions
{
    public static async Task<PagingList<T>> GetPagingListAsync<T>(
        this Query query,
        PagingRequest paging,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.Clone().CountAsync<int>(cancellationToken: cancellationToken);

        var items = await query.Clone()
            .Offset(paging.Skip)
            .Limit(paging.Take)
            .GetAsync<T>(cancellationToken: cancellationToken);

        return new PagingList<T>(items.ToList(), paging.Page, paging.PageSize, totalCount);
    }
}
