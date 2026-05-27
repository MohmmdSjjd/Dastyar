namespace Dastyar.Controllers;

public sealed class UpdateFieldDefinitionRequest
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string DataType { get; set; } = "string";
    public bool IsFilterable { get; set; }
    public bool IsSortable { get; set; }
    public bool IsActive { get; set; } = true;
}
