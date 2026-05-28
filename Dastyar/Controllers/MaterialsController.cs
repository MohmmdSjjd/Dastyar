using Dastyar.Application.Common;
using Dastyar.Application.Materials.Commands;
using Dastyar.Application.Materials.Dtos;
using Dastyar.Application.Materials.Queries;
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
[Route("api/materials")]
public sealed class MaterialsController(ISender sender, ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MaterialDto>>> GetMaterials(CancellationToken cancellationToken)
    {
        var items = await sender.Send(new GetMaterialsQuery(), cancellationToken);
        return Ok(items);
    }

    [HttpPost("search")]
    [DynamicSearchAttribute<Material>]
    public async Task<ActionResult<IReadOnlyList<MaterialDto>>> SearchMaterials(
        [FromBody] GetMaterialsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await sender.Send(query, cancellationToken);
        return Ok(items);
    }

    [HttpGet("search/simple")]
    public async Task<ActionResult<IReadOnlyList<MaterialDto>>> SimpleSearch(
        [FromQuery] string q,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<MaterialDto>());
        }

        var allMaterials = await sender.Send(new GetMaterialsQuery(), cancellationToken);
        var searchTerm = q.ToLowerInvariant();
        var filtered = allMaterials
            .Where(m =>
                (m.Code?.ToLowerInvariant() ?? "").Contains(searchTerm) ||
                (m.Name?.ToLowerInvariant() ?? "").Contains(searchTerm) ||
                (m.UnitEffective?.ToLowerInvariant() ?? "").Contains(searchTerm) ||
                m.BasePrice.ToString().Contains(searchTerm) ||
                SearchInDynamicFields(m.DynamicFieldsJson, searchTerm))
            .ToList();

        return Ok(filtered);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateMaterial(
        [FromBody] CreateMaterialCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await sender.Send(command, cancellationToken);
            return Created($"/api/materials/{id}", new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { error = "خطا در ایجاد ماده اولیه" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMaterial(
        Guid id,
        [FromBody] UpdateMaterialRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(
                new UpdateMaterialCommand(
                    id,
                    request.Code,
                    request.Name,
                    request.IsActive,
                    request.BasePrice,
                    request.LastPurchasePrice,
                    request.DailyPurchasePrice,
                    request.Unit,
                    request.CategoryCode,
                    request.DynamicFieldsJson),
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { error = "خطا در ویرایش ماده اولیه" });
        }
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportMaterialsResultDto>> ImportMaterials([FromForm] ImportMaterialsCommand command)
    {
        try
        {
            if (command.File is null || command.File.Length == 0)
            {
                return BadRequest(new { error = "فایل Excel الزامی است." });
            }

            var result = await sender.Send(new ImportMaterialsCommand(command.File));
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { error = "خطا در ایمپورت مواد اولیه" });
        }
    }

    [HttpPost("import-last-price-update-only")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ImportOutcomeDto>> ImportLastPriceUpdateOnly(
        [FromForm] IFormFile file,
        [FromQuery] Guid? categoryId,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "فایل Excel الزامی است." });
        }

        var (rows, _) = await ExcelTableReader.ReadAsync(file, cancellationToken);
        var warnings = new List<ImportWarningDto>();
        var updated = 0;
        var skipped = 0;
        HashSet<Guid> allowedMaterialIds = [];

        if (categoryId is not null)
        {
            allowedMaterialIds = await GetCategoryAndChildrenIds(categoryId.Value, cancellationToken);
        }

        foreach (var (row, rowNumber) in rows.Select((value, index) => (value, index + 2)))
        {
            if (!row.TryGetValue("Code", out var code) || string.IsNullOrWhiteSpace(code))
            {
                skipped++;
                warnings.Add(new ImportWarningDto(rowNumber, null, "Code is required."));
                continue;
            }

            var material = await db.Materials.FirstOrDefaultAsync(x => x.Code == code && !x.Deleted, cancellationToken);
            if (material is null)
            {
                skipped++;
                warnings.Add(new ImportWarningDto(rowNumber, code, "Material not found."));
                continue;
            }

            if (allowedMaterialIds.Count > 0 && !allowedMaterialIds.Contains(material.Id))
            {
                skipped++;
                warnings.Add(new ImportWarningDto(rowNumber, code, "Material is outside selected category."));
                continue;
            }

            var price = ReadInt(row, "LastPurchasePrice") ?? ReadInt(row, "BasePrice");
            if (price is null)
            {
                skipped++;
                warnings.Add(new ImportWarningDto(rowNumber, code, "LastPurchasePrice is required."));
                continue;
            }

            AddPriceLog(material.Id, "LastPurchasePrice", material.LastPurchasePrice, price.Value, "excel");
            material.LastPurchasePrice = price.Value;
            material.BasePrice = price.Value;
            updated++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new ImportOutcomeDto(0, updated, skipped, warnings));
    }

    [HttpPatch("{id:guid}/prices")]
    public async Task<IActionResult> UpdatePrices(Guid id, [FromBody] UpdateMaterialPricesRequest request, CancellationToken cancellationToken)
    {
        var material = await db.Materials.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, cancellationToken);
        if (material is null)
        {
            return NotFound(new { error = "ماده اولیه پیدا نشد." });
        }

        if (request.LastPurchasePrice.HasValue)
        {
            AddPriceLog(material.Id, "LastPurchasePrice", material.LastPurchasePrice, request.LastPurchasePrice.Value, request.ChangeSource);
            material.LastPurchasePrice = request.LastPurchasePrice.Value;
            material.BasePrice = request.LastPurchasePrice.Value;
        }

        if (request.DailyPurchasePrice.HasValue)
        {
            AddPriceLog(material.Id, "DailyPurchasePrice", material.DailyPurchasePrice, request.DailyPurchasePrice.Value, request.ChangeSource);
            material.DailyPurchasePrice = request.DailyPurchasePrice.Value;
        }

        if (request.LastPurchasePrice.HasValue && !request.DailyPurchasePrice.HasValue)
        {
            material.DailyPurchasePrice = request.LastPurchasePrice.Value;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { id = material.Id });
    }

    [HttpPost("daily-price/percent")]
    public async Task<IActionResult> UpdateDailyPriceByPercent([FromBody] MaterialPricePercentRequest request, CancellationToken cancellationToken)
    {
        var categoryIds = request.IncludeChildren
            ? await GetCategoryAndChildrenIds(request.CategoryId, cancellationToken)
            : new HashSet<Guid> { request.CategoryId };

        var materials = await db.Materials
            .Where(x => x.CategoryId != null && categoryIds.Contains(x.CategoryId.Value) && !x.Deleted)
            .ToListAsync(cancellationToken);

        foreach (var material in materials)
        {
            var previous = material.DailyPurchasePrice;
            var updated = (int)Math.Round(previous * (1m + request.Percent / 100m), MidpointRounding.AwayFromZero);
            AddPriceLog(material.Id, "DailyPurchasePrice", previous, updated, request.ChangeSource);
            material.DailyPurchasePrice = updated;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new { affected = materials.Count });
    }

    [HttpGet("{id:guid}/usage-products")]
    public async Task<IActionResult> GetUsageProducts(Guid id, CancellationToken cancellationToken)
    {
        var usages = await db.ProductMaterials
            .AsNoTracking()
            .Where(x => x.MaterialId == id)
            .Include(x => x.Product)
            .ToListAsync(cancellationToken);

        var items = usages
            .Select(x => new
            {
                x.ProductId,
                ProductCode = x.Product?.Code,
                ProductName = x.Product?.Name,
                x.Quantity,
                x.WastePercent,
                x.Unit,
                x.SortOrder
            })
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.ProductName)
            .ToList();

        return Ok(new { count = items.Count, items });
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportMaterials([FromBody] ExportMaterialsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await sender.Send(new ExportMaterialsQuery(request?.Fields), cancellationToken);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch
        {
            return StatusCode(500, new { error = "خطا در خروجی‌گیری" });
        }
    }

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

            if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                return false;
            }

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
        catch
        {
            return false;
        }

        return false;
    }

    private void AddPriceLog(Guid materialId, string priceType, int oldValue, int newValue, string changeSource)
    {
        db.MaterialPriceChangeLogs.Add(new MaterialPriceChangeLog
        {
            Id = Guid.NewGuid(),
            MaterialId = materialId,
            PriceType = priceType,
            OldValue = oldValue,
            NewValue = newValue,
            ChangeSource = changeSource,
            ChangedAtUtc = DateTime.UtcNow
        });
    }

    private async Task<HashSet<Guid>> GetCategoryAndChildrenIds(Guid categoryId, CancellationToken cancellationToken)
    {
        var categoryRows = await db.Categories
            .AsNoTracking()
            .Where(x => !x.Deleted && x.Scope == CategoryScope.Material)
            .Select(x => new { x.Id, x.ParentCategoryId })
            .ToListAsync(cancellationToken);

        var result = new HashSet<Guid>();
        var stack = new Stack<Guid>();
        stack.Push(categoryId);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!result.Add(current))
            {
                continue;
            }

            foreach (var child in categoryRows.Where(x => x.ParentCategoryId == current))
            {
                stack.Push(child.Id);
            }
        }

        return result;
    }

    private static int? ReadInt(Dictionary<string, string> row, string key)
        => row.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : null;
}
