namespace MyOS.Core.Application.Abstractions.Pagination
{
    public sealed class PagingList<T>
    {
        public IReadOnlyCollection<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages =>
            (int)Math.Ceiling((double)TotalCount / PageSize);

        public bool HasNextPage => Page < TotalPages;
        public bool HasPreviousPage => Page > 1;

        public PagingList(
            IReadOnlyCollection<T> items,
            int page,
            int pageSize,
            int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
