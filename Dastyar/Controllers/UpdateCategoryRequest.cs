namespace Dastyar.Controllers;

public sealed record UpdateCategoryRequest(
    string Code,
    string Name,
    string? ParentCategoryCode,
    string? UnitDefault);
