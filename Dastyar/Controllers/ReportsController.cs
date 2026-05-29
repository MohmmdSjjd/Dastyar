using Dastyar.Domain.Entities;
using Dastyar.Domain.Enums;
using Dastyar.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public sealed class ReportsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("product-cost")]
    public async Task<IActionResult> ProductCost([FromQuery] Guid productId, [FromQuery] string priceBasis = "last", CancellationToken cancellationToken = default)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == productId && !x.Deleted, cancellationToken);
        if (product is null)
        {
            return NotFound(new { error = "محصول پیدا نشد." });
        }

        var basis = string.Equals(priceBasis, "daily", StringComparison.OrdinalIgnoreCase)
            ? PriceBasis.Daily
            : PriceBasis.LastPurchase;

        var lines = await db.ProductMaterials
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Include(x => x.Material)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var dtoLines = lines.Select(item =>
        {
            var unitPrice = basis == PriceBasis.Daily
                ? item.Material?.DailyPurchasePrice ?? 0
                : item.Material?.LastPurchasePrice ?? 0;
            var effectiveQuantity = item.Quantity * (1 + (item.WastePercent / 100m));
            var lineTotal = (int)Math.Round(unitPrice * effectiveQuantity, MidpointRounding.AwayFromZero);

            return new ProductMaterialLineDto(
                item.Id,
                item.MaterialId,
                item.Material?.Code,
                item.Material?.Name,
                item.Quantity,
                item.WastePercent,
                item.Unit ?? item.Material?.Unit,
                unitPrice,
                lineTotal);
        }).ToList();

        var total = dtoLines.Sum(x => x.LineTotal);
        return Ok(new ProductCostReportDto(
            product.Id,
            product.Code,
            product.Name,
            basis.ToString().ToLowerInvariant(),
            total,
            total,
            dtoLines));
    }

    [HttpGet("material-identity")]
    public async Task<IActionResult> MaterialIdentity([FromQuery] Guid materialId, [FromQuery] string priceBasis = "last", CancellationToken cancellationToken = default)
    {
        var material = await db.Materials
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == materialId && !x.Deleted, cancellationToken);

        if (material is null)
        {
            return NotFound(new { error = "ماده اولیه پیدا نشد." });
        }

        var basis = string.Equals(priceBasis, "daily", StringComparison.OrdinalIgnoreCase)
            ? PriceBasis.Daily
            : PriceBasis.LastPurchase;

        var unitPrice = basis == PriceBasis.Daily
            ? material.DailyPurchasePrice
            : material.LastPurchasePrice;

        var usages = await db.ProductMaterials
            .AsNoTracking()
            .Where(x => x.MaterialId == materialId)
            .Include(x => x.Product)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Product != null ? x.Product.Name : string.Empty)
            .ToListAsync(cancellationToken);

        var usedInProducts = usages.Select(item =>
        {
            var effectiveQuantity = item.Quantity * (1 + (item.WastePercent / 100m));
            var lineTotal = (int)Math.Round(unitPrice * effectiveQuantity, MidpointRounding.AwayFromZero);

            return new MaterialUsageProductDto(
                item.ProductId,
                item.Product?.Code,
                item.Product?.Name,
                item.Quantity,
                item.WastePercent,
                item.Unit ?? material.Unit,
                unitPrice,
                lineTotal);
        }).ToList();

        return Ok(new MaterialIdentityReportDto(
            material.Id,
            material.Code,
            material.Name,
            material.Category?.Code,
            material.Category?.Name,
            basis.ToString().ToLowerInvariant(),
            unitPrice,
            usedInProducts.Select(x => x.ProductId).Distinct().Count(),
            usedInProducts));
    }

    [HttpGet("product-identity")]
    public async Task<IActionResult> ProductIdentity([FromQuery] Guid productId, [FromQuery] string priceBasis = "last", CancellationToken cancellationToken = default)
    {
        var product = await db.Products.AsNoTracking().Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == productId && !x.Deleted, cancellationToken);
        if (product is null)
        {
            return NotFound(new { error = "محصول پیدا نشد." });
        }

        var basis = string.Equals(priceBasis, "daily", StringComparison.OrdinalIgnoreCase)
            ? PriceBasis.Daily
            : PriceBasis.LastPurchase;

        var lines = await BuildFormulaLines(productId, basis, cancellationToken);
        var materialCost = lines.Sum(x => x.LineTotal);

        return Ok(new ProductIdentityReportDto(
            product.Id,
            product.Code,
            product.Name,
            product.Category?.Code,
            product.Category?.Name,
            basis.ToString().ToLowerInvariant(),
            materialCost,
            materialCost,
            lines));
    }
    private async Task<IReadOnlyList<ProductMaterialLineDto>> BuildFormulaLines(Guid productId, PriceBasis priceBasis, CancellationToken cancellationToken)
    {
        var lines = await db.ProductMaterials
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Include(x => x.Material)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return lines.Select(item =>
        {
            var unitPrice = priceBasis == PriceBasis.Daily
                ? item.Material?.DailyPurchasePrice ?? 0
                : item.Material?.LastPurchasePrice ?? 0;
            var effectiveQuantity = item.Quantity * (1 + (item.WastePercent / 100m));
            var lineTotal = (int)Math.Round(unitPrice * effectiveQuantity, MidpointRounding.AwayFromZero);

            return new ProductMaterialLineDto(
                item.Id,
                item.MaterialId,
                item.Material?.Code,
                item.Material?.Name,
                item.Quantity,
                item.WastePercent,
                item.Unit ?? item.Material?.Unit,
                unitPrice,
                lineTotal);
        }).ToList();
    }
}
