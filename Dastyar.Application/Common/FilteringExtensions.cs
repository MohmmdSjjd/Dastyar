using Dastyar.Domain.Common;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace Dastyar.Application.Common;

public static class FilteringExtensions
{
    public static IQueryable<TEntity> ApplySearchFilters<TEntity>(
        this IQueryable<TEntity> source,
        IEnumerable<SearchFilter>? filters,
        string[] allowedFields)
        where TEntity : class
    {
        if (filters is null || !filters.Any())
        {
            return source;
        }

        var predicate = filters.BuildSearchPredicate<TEntity>(allowedFields);
        return predicate is null ? source : source.Where(predicate);
    }

    public static Expression<Func<TEntity, bool>>? BuildSearchPredicate<TEntity>(
        this IEnumerable<SearchFilter> filters,
        string[] allowedFields)
        where TEntity : class
    {
        var items = filters.Where(filter => filter is not null && !string.IsNullOrWhiteSpace(filter.Field)).ToList();
        if (!items.Any())
        {
            return null;
        }

        var entityType = typeof(TEntity);
        var parameter = Expression.Parameter(entityType, "e");
        Expression? combined = null;

        foreach (var filter in items)
        {
            if (!allowedFields.Contains(filter.Field, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            var property = entityType.GetProperty(filter.Field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            Expression propertyExpression;
            Type targetType;
            object? typedValue;

            if (property is null)
            {
                // try dynamic JSON field fallback (DynamicFieldsJson)
                var dynProp = entityType.GetProperty("DynamicFieldsJson", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (dynProp is null)
                {
                    continue;
                }

                propertyExpression = Expression.Property(parameter, dynProp);
                targetType = typeof(string);
                // convert value to string
                if (!TryConvertFilterValue(filter.Value, targetType, out typedValue))
                {
                    continue;
                }

                var textValue = typedValue?.ToString() ?? string.Empty;

                // For exact equality on JSON we search for the key/value pair pattern
                if (filter.Operator == SearchOperator.Equal || filter.Operator == SearchOperator.NotEqual)
                {
                    var pattern = $"\"{filter.Field}\":\"{textValue}\"";
                    // use contains on JSON
                    typedValue = pattern;
                    // treat as Contains
                    var jsonComparison = BuildComparison(propertyExpression, targetType, SearchOperator.Contains, typedValue);
                    if (jsonComparison is null)
                    {
                        continue;
                    }
                    // if NotEqual, invert
                    if (filter.Operator == SearchOperator.NotEqual)
                    {
                        jsonComparison = Expression.Not(jsonComparison);
                    }

                    combined = combined is null ? jsonComparison : Expression.AndAlso(combined, jsonComparison);
                    continue;
                }

                // For other string-like operators, use contains/starts/ends on JSON text
                if (filter.Operator == SearchOperator.Contains || filter.Operator == SearchOperator.StartsWith || filter.Operator == SearchOperator.EndsWith)
                {
                    if (!TryConvertFilterValue(filter.Value, targetType, out typedValue))
                    {
                        continue;
                    }
                    var jsonTextComparison = BuildComparison(propertyExpression, targetType, filter.Operator, typedValue);
                    if (jsonTextComparison is null) continue;
                    combined = combined is null ? jsonTextComparison : Expression.AndAlso(combined, jsonTextComparison);
                    continue;
                }

                // Other operators (>, <, etc.) are not supported on JSON fields
                continue;
            }

            var propertyType = property.PropertyType;
            targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (!TryConvertFilterValue(filter.Value, targetType, out typedValue))
            {
                continue;
            }

            propertyExpression = Expression.Property(parameter, property);
            Expression comparison = BuildComparison(propertyExpression, targetType, filter.Operator, typedValue);
            if (comparison is null)
            {
                continue;
            }

            combined = combined is null ? comparison : Expression.AndAlso(combined, comparison);
        }

        if (combined is null)
        {
            return null;
        }

        return Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
    }

    public static IQueryable<TEntity> ApplySearchFilters<TEntity>(
        this IQueryable<TEntity> source,
        IEnumerable<SearchFilter>? filters)
        where TEntity : class, IFilterable, new()
    {
        // مرحله: آماده سازی فیلدهای مجاز برای فیلتر
        var allowedFields = new TEntity().GetFilterableFields();
        return source.ApplySearchFilters(filters, allowedFields);
    }

    public static Expression<Func<TEntity, bool>>? BuildSearchPredicate<TEntity>(
        this IEnumerable<SearchFilter> filters)
        where TEntity : class, IFilterable, new()
    {
        // مرحله: آماده سازی فیلدهای مجاز برای فیلتر
        var allowedFields = new TEntity().GetFilterableFields();
        return filters.BuildSearchPredicate<TEntity>(allowedFields);
    }

    private static Expression? BuildComparison(Expression propertyExpression, Type targetType, SearchOperator op, object? value)
    {
        var constant = Expression.Constant(value, targetType);
        if (op == SearchOperator.Contains || op == SearchOperator.StartsWith || op == SearchOperator.EndsWith)
        {
            if (targetType != typeof(string))
            {
                return null;
            }

            var methodName = op switch
            {
                SearchOperator.Contains => nameof(string.Contains),
                SearchOperator.StartsWith => nameof(string.StartsWith),
                SearchOperator.EndsWith => nameof(string.EndsWith),
                _ => null
            };

            if (methodName is null)
            {
                return null;
            }

            var nullSafeExpression = Expression.Coalesce(propertyExpression, Expression.Constant(string.Empty));
            var method = typeof(string).GetMethod(methodName, new[] { typeof(string) })!;
            return Expression.Call(nullSafeExpression, method, constant);
        }

        if (value is null)
        {
            return op switch
            {
                SearchOperator.Equal => Expression.Equal(propertyExpression, Expression.Constant(null, propertyExpression.Type)),
                SearchOperator.NotEqual => Expression.NotEqual(propertyExpression, Expression.Constant(null, propertyExpression.Type)),
                _ => null
            };
        }

        if (propertyExpression.Type != targetType)
        {
            propertyExpression = Expression.Convert(propertyExpression, targetType);
        }

        return op switch
        {
            SearchOperator.Equal => Expression.Equal(propertyExpression, constant),
            SearchOperator.NotEqual => Expression.NotEqual(propertyExpression, constant),
            SearchOperator.GreaterThan => Expression.GreaterThan(propertyExpression, constant),
            SearchOperator.GreaterThanOrEqual => Expression.GreaterThanOrEqual(propertyExpression, constant),
            SearchOperator.LessThan => Expression.LessThan(propertyExpression, constant),
            SearchOperator.LessThanOrEqual => Expression.LessThanOrEqual(propertyExpression, constant),
            _ => null,
        };
    }

    private static bool TryConvertFilterValue(object? value, Type targetType, out object? converted)
    {
        converted = null;
        if (value is null)
        {
            return true;
        }

        if (value is JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == JsonValueKind.Null)
            {
                return true;
            }

            value = jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString() ?? string.Empty,
                JsonValueKind.Number => jsonElement.GetRawText(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => jsonElement.ToString() ?? string.Empty,
            };
        }

        if (targetType == typeof(string))
        {
            converted = value.ToString();
            return true;
        }

        if (targetType.IsEnum)
        {
            if (value is string stringValue)
            {
                if (Enum.TryParse(targetType, stringValue, true, out var enumValue))
                {
                    converted = enumValue;
                    return true;
                }
            }

            if (TryChangeType(value, typeof(int), out var numericValue) && numericValue is not null && Enum.IsDefined(targetType, numericValue))
            {
                converted = Enum.ToObject(targetType, numericValue);
                return true;
            }

            return false;
        }

        if (targetType == typeof(Guid))
        {
            var textValue = value?.ToString() ?? string.Empty;
            if (Guid.TryParse(textValue, out var guidValue))
            {
                converted = guidValue;
                return true;
            }

            return false;
        }

        if (targetType == typeof(bool))
        {
            var textValue = value?.ToString() ?? string.Empty;
            if (bool.TryParse(textValue, out var boolValue))
            {
                converted = boolValue;
                return true;
            }

            if (int.TryParse(textValue, out var intValue))
            {
                converted = intValue != 0;
                return true;
            }

            return false;
        }

        return TryChangeType(value, targetType, out converted);
    }

    private static bool TryChangeType(object? value, Type targetType, out object? result)
    {
        try
        {
            result = Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }
}
