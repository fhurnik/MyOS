namespace MyOS.Core.Application.Abstractions.Pagination
{
    public record PagingRequest
    {
        private const int MaxPageSize = 100;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
        public string? OrderBy { get; init; }
        public bool OrderByDesc { get; set; } = false;

        internal int Skip => Math.Max(0, (Page - 1) * Take);
        internal int Take => Math.Clamp(PageSize, 1, MaxPageSize);
    }
}
