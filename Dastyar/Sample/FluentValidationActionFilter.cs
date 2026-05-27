/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Csis.Template.WebApi.Filters;

/// <summary>
/// Action filter to validate input using FluentValidation
/// </summary>
public sealed class FluentValidationActionFilter : IAsyncActionFilter
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    /// <exception cref="ValidationException"></exception>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
        var services = context.HttpContext.RequestServices;
        var token = context.HttpContext.RequestAborted;

        foreach ( var arg in context.ActionArguments.Values ) {
            if ( arg is null ) {
                continue;
            }

            var argType = arg.GetType();

            var validatorType = typeof(IValidator<>).MakeGenericType(argType);
            var validators = services.GetServices(validatorType)
                .Where(x => x is IValidator)
                .Select(x => x as IValidator)
                .ToArray();

            if ( validators.Length > 0 ) {
                var validationContext = new ValidationContext<object>(arg);
                var validationResults = await Task.WhenAll(
                    validators.Select(v => v.ValidateAsync(validationContext))
                );

                var failures = validationResults
                    .SelectMany(result => result.Errors)
                    .Where(f => f != null)
                    .ToList();

                if ( failures.Count > 0 ) {
                    throw new ValidationException(failures);
                }
            }
        }

        await next();
    }
}
