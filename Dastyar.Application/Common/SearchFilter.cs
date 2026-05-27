namespace Dastyar.Application.Common;

public sealed record SearchFilter
{
    public string Field { get; init; } = string.Empty;
    public SearchOperator Operator { get; init; } = SearchOperator.Equal;
    public object? Value { get; init; }
}
