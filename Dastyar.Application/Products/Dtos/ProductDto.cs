using Dastyar.Application.Common.Mapping;
using Dastyar.Domain.Entities;

namespace Dastyar.Application.Products.Dtos;

public sealed record ProductDto : BaseDto<ProductDto, Product, Guid>
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string? Unit { get; init; }
    public string? CategoryCode { get; init; }
    public string? CategoryName { get; init; }
    // مقادیر داینامیک ذخیره شده به صورت JSON
    public string? DynamicFieldsJson { get; init; }
}
