namespace Dastyar.Controllers;

public sealed record UpdateMaterialAddonAssignmentRequest(
    string AddonMaterialCode,
    string? TargetCategoryCode,
    string? TargetMaterialCode,
    int Quantity,
    string? UnitOverride);
