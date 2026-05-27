namespace Dastyar.Application.Common;

public record BaseSearchQuery
{
    private static readonly List<int> AllowedPageSizes = new() { 5, 10, 15, 20, 50 };
    private int _pageSize = DefaultPageSize;
    private string _sortBy = DefaultSort;

    public const string DefaultSort = "-Id";
    public const int DefaultPageSize = 5;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = AllowedPageSizes.Contains(value) ? value : DefaultPageSize;
    }

    public int PageIndex { get; init; } = 1;

    public List<SearchFilter> SearchFilters { get; init; } = new();

    public string SortBy
    {
        get => string.IsNullOrWhiteSpace(_sortBy) ? DefaultSort : _sortBy;
        init => _sortBy = string.IsNullOrWhiteSpace(value) ? DefaultSort : value;
    }
}
