namespace Dastyar.Application.Materials.Dtos;

public sealed record MaterialAddonAssignmentDto
{
    public Guid Id { get; init; }
    public Guid AddonMaterialId { get; init; }
    public string? AddonMaterialCode { get; init; }
    public string? AddonMaterialName { get; init; }
    public Guid? TargetCategoryId { get; init; }
    public string? TargetCategoryCode { get; init; }
    public string? TargetCategoryName { get; init; }
    public Guid? TargetMaterialId { get; init; }
    public string? TargetMaterialCode { get; init; }
    public string? TargetMaterialName { get; init; }
    public int Quantity { get; init; }
    public string? UnitOverride { get; init; }
}
