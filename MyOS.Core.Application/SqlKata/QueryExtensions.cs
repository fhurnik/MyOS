using System.Text.RegularExpressions;
using MyOS.Core.Application.Abstractions.Pagination;
using MyOS.Core.Application.Abstractions.Results;
using MyOS.Core.Application.Errors;
using SqlKata;
using SqlKata.Execution;

namespace MyOS.Core.Application.SqlKata;

public static class QueryExtensions
{
    public static async Task<Result<PagingList<T>>> GetPagingListAsync<T>(
        this Query query,
        PagingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(request.OrderBy))
        {
            var allowedColumns = typeof(T).GetProperties().Select(p => p.Name);
            if (!allowedColumns.Any(name => MatchesOrderByName(request.OrderBy, name)))
                return Result<PagingList<T>>.Failure(PagingErrors.InvalidOrderBy with
                {
                    Parameters = new Dictionary<string, string> { ["column"] = request.OrderBy }
                });
        }

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

        return Result<PagingList<T>>.Success(new PagingList<T>(items.ToList(), request.Page, request.Take, totalCount));
    }

    private static bool MatchesOrderByName(string orderBy, string propertyName) =>
        orderBy.Length == propertyName.Length &&
        char.ToLowerInvariant(orderBy[0]) == char.ToLowerInvariant(propertyName[0]) &&
        orderBy.AsSpan(1).SequenceEqual(propertyName.AsSpan(1));

    private static string ToSnakeCase(string value) =>
        Regex.Replace(value, "(?<=[a-z0-9])([A-Z])", "_$1").ToLower();
}
