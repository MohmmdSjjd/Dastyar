namespace Dastyar.Application.Common;

public static class ResultExtensions
{
    public static Result<TModel> ToResult<TModel>(this TModel model)
        where TModel : class
        => Result<TModel>.Success(model);

    public static PaginatedResult<TDestination> ToPaginatedResult<T, TDestination>(
        this IPagedList<T> pagedList,
        IEnumerable<TDestination> mappedList,
        Dictionary<string, object>? routeValues = null)
        where T : class
        where TDestination : class
    {
        return PaginatedResult<TDestination>.Success(
            mappedList.ToList(),
            pagedList.TotalPages,
            pagedList.TotalCount,
            pagedList.PageIndex,
            pagedList.PageSize,
            routeValues);
    }

    public static PaginatedResult<T> ToPaginatedResult<T>(this IPagedList<T> pagedList, Dictionary<string, object>? routeValues = null)
        where T : class
    {
        return PaginatedResult<T>.Success(
            pagedList.Items.ToList(),
            pagedList.TotalPages,
            pagedList.TotalCount,
            pagedList.PageIndex,
            pagedList.PageSize,
            routeValues);
    }
}
