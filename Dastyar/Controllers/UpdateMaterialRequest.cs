namespace Dastyar.Controllers;

public sealed class UpdateMaterialRequest
{
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public int BasePrice { get; set; }
    public string? Unit { get; set; }
    public string? CategoryCode { get; set; }
    public string? DynamicFieldsJson { get; set; }
}
