using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public sealed class MaterialPriceChangeLog : BaseEntity<Guid>, IFilterable
{
    public Guid MaterialId { get; set; }
    public Material? Material { get; set; }

    public string PriceType { get; set; } = string.Empty;
    public int OldValue { get; set; }
    public int NewValue { get; set; }
    public string ChangeSource { get; set; } = string.Empty;
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? BatchId { get; set; }

    public string[] GetFilterableFields() => new[]
    {
        "MaterialId",
        "PriceType",
        "OldValue",
        "NewValue",
        "ChangeSource",
        "ChangedAtUtc",
        "BatchId",
    };
}
