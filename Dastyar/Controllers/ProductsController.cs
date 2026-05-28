using Dastyar.Application.Common;
using Dastyar.Application.Products.Commands;
using Dastyar.Application.Products.Dtos;
using Dastyar.Application.Products.Queries;
using Dastyar.Domain.Entities;
using Dastyar.Domain.Enums;
using Dastyar.Filters;
using Dastyar.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController(ISender sender, ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        var items = await sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(items);
    }

    [HttpPost("search")]
    [DynamicSearchAttribute<Product>]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> SearchProducts([FromBody] GetProductsQuery query, CancellationToken cancellationToken)
    {
        var items = await sender.Send(query, cancellationToken);
        return Ok(items);
    }

    [HttpGet("search/simple")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> SimpleSearch([FromQuery] string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<ProductDto>());
        }

        var allProducts = await sender.Send(new GetProductsQuery(), cancellationToken);
        var searchTerm = q.ToLowerInvariant();
        var filtered = allProducts
            .Where(p =>
                (p.Code?.ToLowerInvariant() ?? "").Contains(searchTerm) ||
                (p.Name?.ToLowerInvariant() ?? "").Contains(searchTerm) ||
                SearchInDynamicFields(p.DynamicFieldsJson, searchTerm))
            .ToList();

        return Ok(filtered);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateProduct([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var id = await sender.Send(command, cancellationToken);
            return Created($"/api/products/{id}", new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("IX_Products_Code") == true)
            {
                return BadRequest(new { error = "محصول با این کد قبلاً وجود دارد." });
            }

            return StatusCode(500, new { error = "خطا در ایجاد محصول" });
        }
        catch
        {
            return StatusCode(500, new { error = "خطا در ایجاد محصول" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateProductCommand(id, request.Code, request.Name, request.IsActive, request.Unit, request.DynamicFieldsJson);
            await sender.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("IX_Products_Code") == true)
            {
                return BadRequest(new { error = "محصول با این کد قبلاً وجود دارد." });
            }

            return StatusCode(500, new { error = "خطا در ویرایش محصول" });
        }
        catch
        {
            return StatusCode(500, new { error = "خطا در ویرایش محصول" });
        }
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportProductsResultDto>> ImportProducts([FromForm] ImportProductsCommand command)
    {
        try
        {
            if (command.File is null || command.File.Length == 0)
            {
                return BadRequest(new { error = "فایل Excel الزامی است." });
            }

            var result = await sender.Send(new ImportProductsCommand(command.File));
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { error = "خطا در ایمپورت محصولات" });
        }
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportProducts([FromBody] ExportProductsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new ExportProductsQuery(request?.Fields), cancellationToken);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch
        {
            return StatusCode(500, new { error = "خطا در خروجی‌گیری" });
        }
    }

    [HttpGet("{id:guid}/formula")]
    public async Task<IActionResult> GetFormula(Guid id, [FromQuery] string priceBasis = "last", CancellationToken cancellationToken = default)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, cancellationToken);
        if (product is null)
        {
            return NotFound(new { error = "محصول پیدا نشد." });
        }

        var basis = ParsePriceBasis(priceBasis);
        var lines = await BuildFormulaLines(id, basis, cancellationToken);
        var materialCost = lines.Sum(x => x.LineTotal);

        return Ok(new ProductCostReportDto(
            product.Id,
            product.Code,
            product.Name,
            basis.ToString().ToLowerInvariant(),
            materialCost,
            materialCost,
            lines));
    }

    [HttpPost("{id:guid}/formula")]
    public async Task<IActionResult> SaveFormula(Guid id, [FromBody] ProductFormulaSaveRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, cancellationToken);
        if (product is null)
        {
            return NotFound(new { error = "محصول پیدا نشد." });
        }

        var existing = await db.ProductMaterials.Where(x => x.ProductId == id).ToListAsync(cancellationToken);
        db.ProductMaterials.RemoveRange(existing);

        foreach (var item in request.Items.OrderBy(x => x.SortOrder))
        {
            db.ProductMaterials.Add(new ProductMaterial
            {
                Id = Guid.NewGuid(),
                ProductId = id,
                MaterialId = item.MaterialId,
                Quantity = item.Quantity,
                WastePercent = item.WastePercent,
                Unit = item.Unit,
                SortOrder = item.SortOrder
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { count = request.Items.Count });
    }

    private async Task<IReadOnlyList<ProductMaterialLineDto>> BuildFormulaLines(Guid productId, PriceBasis priceBasis, CancellationToken cancellationToken)
    {
        var items = await db.ProductMaterials
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Include(x => x.Material)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return items.Select(item =>
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

    private static PriceBasis ParsePriceBasis(string priceBasis)
        => string.Equals(priceBasis, "daily", StringComparison.OrdinalIgnoreCase)
            ? PriceBasis.Daily
            : PriceBasis.LastPurchase;

    private static bool SearchInDynamicFields(string? dynamicFieldsJson, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(dynamicFieldsJson))
        {
            return false;
        }

        try
        {
            using var json = System.Text.Json.JsonDocument.Parse(dynamicFieldsJson);
            var root = json.RootElement;

            if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                foreach (var property in root.EnumerateObject())
                {
                    var value = property.Value.ValueKind == System.Text.Json.JsonValueKind.String
                        ? property.Value.GetString()
                        : property.Value.ToString();

                    if (value?.ToLowerInvariant().Contains(searchTerm) == true)
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
