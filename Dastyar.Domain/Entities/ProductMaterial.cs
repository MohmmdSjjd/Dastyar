using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public sealed class ProductMaterial : BaseEntity<Guid>, IFilterable
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid MaterialId { get; set; }
    public Material? Material { get; set; }

    public decimal Quantity { get; set; } = 1;
    public decimal WastePercent { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }

    public string[] GetFilterableFields() => new[]
    {
        "ProductId",
        "MaterialId",
        "Quantity",
        "WastePercent",
        "Unit",
        "SortOrder",
    };
}
