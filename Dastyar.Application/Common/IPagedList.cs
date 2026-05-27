namespace Dastyar.Application.Common;

public interface IPagedList
{
    int TotalPages { get; }
    int PageIndex { get; }
    int PageSize { get; }
    int TotalCount { get; }
    string SortExpression { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

public interface IPagedList<out T> : IPagedList
    where T : class
{
    IReadOnlyList<T> Items { get; }
}
