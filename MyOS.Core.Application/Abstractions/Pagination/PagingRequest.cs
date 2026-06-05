namespace MyOS.Core.Application.Abstractions.Pagination
{
    public record PagingRequest
    {
        private const int MaxPageSize = 100;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? OrderBy { get; init; }
        public bool OrderByDesc { get; set; } = false;

        internal int Skip => (Page - 1) * PageSize;
        internal int Take => PageSize > MaxPageSize
            ? MaxPageSize
            : PageSize;
    }
}
