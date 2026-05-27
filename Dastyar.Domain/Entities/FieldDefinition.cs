using Dastyar.Domain.Common;

namespace Dastyar.Domain.Entities;

public sealed class FieldDefinition : BaseEntity<Guid>, IFilterable
{
    public string Name { get; set; } = string.Empty; // internal key
    public string? DisplayName { get; set; }
    public string DataType { get; set; } = "string";
    public bool IsFilterable { get; set; } = true;
    public bool IsSortable { get; set; } = false;
    public bool IsActive { get; set; } = true;

    public string[] GetFilterableFields() => new[] { "Name", "DisplayName", "DataType", "IsFilterable", "IsSortable", "IsActive" };
}
