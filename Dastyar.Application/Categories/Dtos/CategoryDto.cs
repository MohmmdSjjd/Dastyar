using Dastyar.Application.Common.Mapping;
using Dastyar.Domain.Entities;
using Dastyar.Domain.Enums;

namespace Dastyar.Application.Categories.Dtos;

public sealed record CategoryDto : BaseDto<CategoryDto, Category, Guid>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public CategoryScope Scope { get; init; }
    public bool IsActive { get; init; }
    public int SortOrder { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public string? ParentCategoryCode { get; init; }
    public string? UnitDefault { get; init; }
}
