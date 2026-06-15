using System.Text.RegularExpressions;
using MyOS.Core.Application.Abstractions.Pagination;
using SqlKata;
using SqlKata.Execution;

namespace MyOS.Core.Application.SqlKata;

public static class QueryExtensions
{
    public static async Task<PagingList<T>> GetPagingListAsync<T>(
        this Query query,
        PagingRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.Clone().CountAsync<int>(cancellationToken: cancellationToken);

        var queryClone = query.Clone();
        if (!string.IsNullOrEmpty(request.OrderBy))
        {
            var column = ToSnakeCase(request.OrderBy);
            if (request.OrderByDesc)
                queryClone.OrderByDesc(column);
            else
                queryClone.OrderBy(column);
        }

        var items = await queryClone
            .Offset(request.Skip)
            .Limit(request.Take)
            .GetAsync<T>(cancellationToken: cancellationToken);

        return new PagingList<T>(items.ToList(), request.Page, request.Take, totalCount);
    }

    private static string ToSnakeCase(string value) =>
        Regex.Replace(value, "(?<=[a-z0-9])([A-Z])", "_$1").ToLower();
}
