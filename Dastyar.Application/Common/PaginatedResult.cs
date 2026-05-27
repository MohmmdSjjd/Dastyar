namespace Dastyar.Application.Common;

public sealed record PaginatedResult<T>(bool Succeeded, IReadOnlyList<T> Items, int TotalPages, int TotalCount, int PageIndex, int PageSize, Dictionary<string, object>? RouteValues)
    where T : class
{
    public static PaginatedResult<T> Success(IReadOnlyList<T> items, int totalPages, int totalCount, int pageIndex, int pageSize, Dictionary<string, object>? routeValues)
        => new(true, items, totalPages, totalCount, pageIndex, pageSize, routeValues);
}
