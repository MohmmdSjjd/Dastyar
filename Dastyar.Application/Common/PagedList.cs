namespace Dastyar.Application.Common;

public sealed class PagedList<T> : IPagedList<T> where T : class
{
    public PagedList(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize, string sortExpression)
    {
        Items = items;
        TotalCount = totalCount;
        PageIndex = pageIndex <= 0 ? 1 : pageIndex;
        PageSize = pageSize <= 0 ? 1 : pageSize;
        SortExpression = sortExpression ?? string.Empty;
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
    }

    public IReadOnlyList<T> Items { get; }
    public int TotalPages { get; }
    public int PageIndex { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public string SortExpression { get; }
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;
}
