using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public sealed class MaterialAddonAssignment : BaseEntity<Guid>, IFilterable
{
    public Guid AddonMaterialId { get; set; }
    public Material? AddonMaterial { get; set; }

    public Guid? TargetCategoryId { get; set; }
    public Category? TargetCategory { get; set; }

    public Guid? TargetMaterialId { get; set; }
    public Material? TargetMaterial { get; set; }

    public int Quantity { get; set; } = 1;
    public string? UnitOverride { get; set; }

    public string[] GetFilterableFields() => new[]
    {
        "AddonMaterialId",
        "TargetCategoryId",
        "TargetMaterialId",
        "Quantity",
    };
}
