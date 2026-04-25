namespace MyOS.Core.Application.Abstractions.Pagination
{
    public class PagingRequest
    {
        private const int MaxPageSize = 100;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;

        public int Skip => (Page - 1) * PageSize;
        public int Take => PageSize > MaxPageSize
            ? MaxPageSize
            : PageSize;
    }
}
