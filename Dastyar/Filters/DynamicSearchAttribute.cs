using Dastyar.Application.Common;
using Dastyar.Domain.Common;
using Dastyar.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dastyar.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class DynamicSearchAttribute<TEntity, TKey> : Attribute, IAsyncActionFilter
    where TEntity : BaseEntity<TKey>, IFilterable
    where TKey : IEquatable<TKey>
{
    public DynamicSearchAttribute()
    {
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // مرحله: بررسی ورودی برای وجود کوئری جستجو
        var query = context.ActionArguments.Values.OfType<BaseSearchQuery>().FirstOrDefault();
        if (query is null)
        {
            throw new InvalidOperationException($"Search action argument must inherit from '{nameof(BaseSearchQuery)}'.");
        }

        if (query.SearchFilters is null || query.SearchFilters.Count == 0)
        {
            await next();
            return;
        }

        // get allowed fields from service if available, otherwise fallback to entity-defined fields
        var svc = context.HttpContext.RequestServices.GetService(typeof(Dastyar.Application.Common.Interfaces.IFieldDefinitionService)) as Dastyar.Application.Common.Interfaces.IFieldDefinitionService;
        string[] allowedFieldsFromService = Array.Empty<string>();
        if (svc is not null)
        {
            try { allowedFieldsFromService = await svc.GetFilterableFieldNamesAsync(); } catch { allowedFieldsFromService = Array.Empty<string>(); }
        }

        var allowedSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (allowedFieldsFromService.Length > 0)
        {
            foreach (var f in allowedFieldsFromService) if (!string.IsNullOrWhiteSpace(f)) allowedSet.Add(f);
        }
        else
        {
            var fields = Activator.CreateInstance<TEntity>()?.GetFilterableFields() ?? Array.Empty<string>();
            foreach (var f in fields) if (!string.IsNullOrWhiteSpace(f)) allowedSet.Add(f);
        }

        // validate filters
        for (var i = 0; i < query.SearchFilters.Count; i++)
        {
            var filter = query.SearchFilters[i];
            if (filter is null || string.IsNullOrWhiteSpace(filter.Field))
            {
                continue;
            }

            if (filter.Value is null)
            {
                context.Result = new JsonResult(new ValidationProblemDetails
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Errors = { [$"searchFilters[{i}].value"] = new[] { "Null value for search is not allowed." } }
                })
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity
                };
                return;
            }

            if (!allowedSet.Contains(filter.Field))
            {
                context.Result = new JsonResult(new ValidationProblemDetails
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Errors = { [$"searchFilters[{i}].field"] = new[] { $"'{filter.Field}' is not a valid search filter." } }
                })
                {
                    StatusCode = StatusCodes.Status422UnprocessableEntity
                };
                return;
            }
        }

        await next();
    }
}

public sealed class DynamicSearchAttribute<TEntity> : DynamicSearchAttribute<TEntity, Guid>
    where TEntity : BaseEntity<Guid>, IFilterable
{ }
