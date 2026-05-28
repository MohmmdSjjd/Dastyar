namespace Dastyar.Controllers;

public sealed class UpdateProductRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Unit { get; set; }
    public string? DynamicFieldsJson { get; set; }
}
