namespace Dastyar.Application.Materials.Dtos;

public sealed record MaterialDto
{
    public Guid Id { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public string? CategoryCode { get; init; }
    public string? CategoryName { get; init; }
    public int BasePrice { get; init; }
    public int AddonTotalPrice { get; init; }
    public int FinalPrice { get; init; }
    public string? Unit { get; init; }
    public string? UnitEffective { get; init; }
    public string? DynamicFieldsJson { get; init; }
    public IReadOnlyList<MaterialAppliedAddonDto> AppliedAddons { get; init; } = [];
}

public sealed record MaterialAppliedAddonDto
{
    public Guid AssignmentId { get; init; }
    public Guid AddonMaterialId { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public int Quantity { get; init; }
    public string? Unit { get; init; }
    public int UnitPrice { get; init; }
    public int TotalPrice { get; init; }
}
