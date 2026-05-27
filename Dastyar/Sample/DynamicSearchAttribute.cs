/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

using Csis.Template.Application.Common;
using Csis.Template.Domain.Common;
using Csis.Utilities.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Csis.Template.WebApi.Filters;

/// <summary>
/// Attribute to define dynamic search filter specifications
/// </summary>
/// <typeparam name="TEntity">Entity type that dynamic search is applied on</typeparam>
/// <typeparam name="TKey">Primary key type of entity</typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal class DynamicSearchAttribute<TEntity, TKey> : Attribute, IActionFilter
    where TEntity : class, IFilterable, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private readonly List<string> _fields;

    /// <summary>
    /// Instantiate the attribute
    /// </summary>
    public DynamicSearchAttribute() {
        var fields = Activator.CreateInstance<TEntity>().GetFilterableFields();

        _fields = new List<string>(fields.Length);

        foreach ( var field in fields ) {
            if ( !field.HasValue() ) {
                continue;
            }

            _fields.Add(field.Trim().ToLower());
        }
    }

    /// <summary>
    /// Runs before action executed
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuting(ActionExecutingContext context) {
        if ( context.ActionArguments.Count != 1 ) {
            throw new Exception("Search action must have only one argument");
        }

        var query = context.ActionArguments.First().Value as BaseSearchQuery ??
            throw new Exception($"Search action argument must inherit from '{nameof(BaseSearchQuery)}' to include dynamic search filters");

        if ( query.SearchFilters is not null && query.SearchFilters.Count > 0 ) {

            for ( var i = 0; i < query.SearchFilters.Count; i++ ) {
                var filter = query.SearchFilters[i];
                if ( filter is null || !filter.Field.HasValue() ) {
                    continue;
                }

                if ( filter.Value is null ) {
                    var problemDetails = new ValidationProblemDetails {
                        Title = "Validation failed",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Errors = new Dictionary<string, string[]> {
                            { $"searchFilters[{i}].value", ["Null value for search is not allowed"] }
                        }
                    };

                    context.Result = new JsonResult(problemDetails) {
                        StatusCode = StatusCodes.Status422UnprocessableEntity
                    };

                    return;
                }

                if ( !_fields.Contains(filter.Field.ToLower()) ) {
                    var problemDetails = new ValidationProblemDetails {
                        Title = "Validation failed",
                        Status = StatusCodes.Status422UnprocessableEntity,
                        Errors = new Dictionary<string, string[]> {
                            { $"searchFilters[{i}].field", [$"'{filter.Field}' is not a valid search filter"] }
                        }
                    };

                    context.Result = new JsonResult(problemDetails) {
                        StatusCode = StatusCodes.Status422UnprocessableEntity
                    };

                    return;
                }
            }

        }
    }

    /// <summary>
    /// Runs after action executed
    /// </summary>
    /// <param name="context"></param>
    public void OnActionExecuted(ActionExecutedContext context) { }
}

/// <summary>
/// Attribute to define dynamic search filter specifications
/// </summary>
/// <typeparam name="TEntity">Entity type that dynamic search is applied on. The primary key type of entity is integer</typeparam>
internal sealed class DynamicSearchAttribute<TEntity> : DynamicSearchAttribute<TEntity, int>
    where TEntity : class, IFilterable, IEntity
{ }
