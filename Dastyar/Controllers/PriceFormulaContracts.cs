namespace Dastyar.Controllers;

public sealed record PriceFormulaMenuDto(
    string Key,
    string Title,
    string Color,
    string Description);

public sealed record CategoryNodeDto(
    Guid Id,
    string Code,
    string Name,
    string Scope,
    bool IsActive,
    int SortOrder,
    Guid? ParentCategoryId,
    string? UnitDefault,
    int Level,
    IReadOnlyList<CategoryNodeDto> Children);

public sealed record UpdateMaterialPricesRequest(
    int? LastPurchasePrice,
    int? DailyPurchasePrice,
    string ChangeSource = "manual");

public sealed record MaterialPricePercentRequest(
    Guid CategoryId,
    decimal Percent,
    bool IncludeChildren = true,
    string ChangeSource = "percent");

public sealed record ImportOutcomeDto(
    int Added,
    int Updated,
    int Skipped,
    IReadOnlyList<ImportWarningDto> Warnings);

public sealed record ImportWarningDto(
    int RowNumber,
    string? Code,
    string Reason);

public sealed record ProductMaterialLineDto(
    Guid Id,
    Guid MaterialId,
    string? MaterialCode,
    string? MaterialName,
    decimal Quantity,
    decimal WastePercent,
    string? Unit,
    int UnitPrice,
    int LineTotal);

public sealed record ProductFormulaSaveRequest(
    IReadOnlyList<ProductFormulaItemRequest> Items);

public sealed record ProductFormulaItemRequest(
    Guid MaterialId,
    decimal Quantity,
    decimal WastePercent,
    string? Unit,
    int SortOrder);

public sealed record ProductCostReportDto(
    Guid ProductId,
    string? ProductCode,
    string? ProductName,
    string PriceBasis,
    int MaterialCost,
    int TotalCost,
    IReadOnlyList<ProductMaterialLineDto> Lines);


public sealed record MaterialUsageProductDto(
    Guid ProductId,
    string? ProductCode,
    string? ProductName,
    decimal Quantity,
    decimal WastePercent,
    string? Unit,
    int UnitPrice,
    int LineTotal);

public sealed record MaterialIdentityReportDto(
    Guid MaterialId,
    string? MaterialCode,
    string? MaterialName,
    string? CategoryCode,
    string? CategoryName,
    string PriceBasis,
    int SelectedUnitPrice,
    int UsageProductCount,
    IReadOnlyList<MaterialUsageProductDto> UsedInProducts);

public sealed record ProductIdentityReportDto(
    Guid ProductId,
    string? ProductCode,
    string? ProductName,
    string? CategoryCode,
    string? CategoryName,
    string PriceBasis,
    int MaterialCost,
    int TotalCost,
    IReadOnlyList<ProductMaterialLineDto> Materials);
