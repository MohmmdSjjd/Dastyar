using Dastyar.Domain.Entities;
using Dastyar.Domain.Enums;
using Dastyar.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/material-categories")]
public sealed class MaterialCategoriesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet("tree")]
    public async Task<ActionResult<IReadOnlyList<CategoryNodeDto>>> GetTree(CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Where(x => x.Scope == CategoryScope.Material && !x.Deleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(BuildTree(categories));
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var parent = await FindParentAsync(request.ParentCategoryCode, cancellationToken);
        if (request.ParentCategoryCode is not null && parent is null)
        {
            return BadRequest(new { error = "دسته‌بندی والد پیدا نشد." });
        }

        var code = request.Code.Trim();
        var duplicate = await db.Categories.AnyAsync(x => x.Code == code, cancellationToken);
        if (duplicate)
        {
            return BadRequest(new { error = $"دسته‌بندی با کد '{code}' قبلاً وجود دارد." });
        }

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name.Trim(),
            Scope = CategoryScope.Material,
            IsActive = request.IsActive,
            SortOrder = request.SortOrder,
            ParentCategoryId = parent?.Id,
            UnitDefault = string.IsNullOrWhiteSpace(request.UnitDefault) ? null : request.UnitDefault.Trim(),
        };

        await db.Categories.AddAsync(category, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"/api/material-categories/{category.Id}", new { category.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, cancellationToken);
        if (category is null)
        {
            return NotFound(new { error = "دسته‌بندی پیدا نشد." });
        }

        var parent = await FindParentAsync(request.ParentCategoryCode, cancellationToken);
        if (request.ParentCategoryCode is not null && parent is null)
        {
            return BadRequest(new { error = "دسته‌بندی والد پیدا نشد." });
        }

        if (!string.Equals(category.Code, request.Code.Trim(), StringComparison.OrdinalIgnoreCase) &&
            await db.Categories.AnyAsync(x => x.Code == request.Code.Trim() && x.Id != category.Id, cancellationToken))
        {
            return BadRequest(new { error = $"دسته‌بندی با کد '{request.Code.Trim()}' قبلاً وجود دارد." });
        }

        category.Code = request.Code.Trim();
        category.Name = request.Name.Trim();
        category.Scope = CategoryScope.Material;
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;
        category.ParentCategoryId = parent?.Id;
        category.UnitDefault = string.IsNullOrWhiteSpace(request.UnitDefault) ? null : request.UnitDefault.Trim();

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted, cancellationToken);
        if (category is null)
        {
            return NotFound(new { error = "دسته‌بندی پیدا نشد." });
        }

        var hasChildren = await db.Categories.AnyAsync(x => x.ParentCategoryId == id && !x.Deleted, cancellationToken);
        var hasMaterials = await db.Materials.AnyAsync(x => x.CategoryId == id && !x.Deleted, cancellationToken);
        if (hasChildren || hasMaterials)
        {
            return BadRequest(new { error = "حذف این دسته‌بندی به‌دلیل وجود زیرشاخه یا مواد اولیه ممکن نیست." });
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{categoryId:guid}/materials/import-new")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ImportOutcomeDto>> ImportNewMaterials(Guid categoryId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(x => x.Id == categoryId && !x.Deleted, cancellationToken);
        if (category is null)
        {
            return NotFound(new { error = "دسته‌بندی پیدا نشد." });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "فایل Excel الزامی است." });
        }

        var (rows, _) = await ExcelTableReader.ReadAsync(file, cancellationToken);
        var warnings = new List<ImportWarningDto>();
        var added = 0;
        var skipped = 0;

        foreach (var (row, index) in rows.Select((value, i) => (value, i + 2)))
        {
            if (!row.TryGetValue("Code", out var code) || string.IsNullOrWhiteSpace(code))
            {
                skipped++;
                warnings.Add(new ImportWarningDto(index, null, "Code is required."));
                continue;
            }

            var exists = await db.Materials.AnyAsync(x => x.Code == code, cancellationToken);
            if (exists)
            {
                skipped++;
                warnings.Add(new ImportWarningDto(index, code, "Material already exists."));
                continue;
            }

            var basePrice = ReadInt(row, "BasePrice");
            var lastPrice = ReadInt(row, "LastPurchasePrice") ?? basePrice ?? 0;
            var dailyPrice = ReadInt(row, "DailyPurchasePrice") ?? lastPrice;

            db.Materials.Add(new Material
            {
                Id = Guid.NewGuid(),
                Code = code.Trim(),
                Name = ReadText(row, "Name"),
                IsActive = ReadBool(row, "IsActive", true),
                CreatedAtUtc = DateTime.UtcNow,
                BasePrice = lastPrice,
                LastPurchasePrice = lastPrice,
                DailyPurchasePrice = dailyPrice,
                Unit = ReadText(row, "Unit"),
                CategoryId = category.Id,
                DynamicFieldsJson = string.Empty
            });

            added++;
        }

        await db.SaveChangesAsync(cancellationToken);
        return Ok(new ImportOutcomeDto(added, 0, skipped, warnings));
    }

    private async Task<Category?> FindParentAsync(string? parentCategoryCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(parentCategoryCode))
        {
            return null;
        }

        return await db.Categories.FirstOrDefaultAsync(
            x => x.Code == parentCategoryCode && x.Scope == CategoryScope.Material && !x.Deleted,
            cancellationToken);
    }

    private static IReadOnlyList<CategoryNodeDto> BuildTree(List<Category> categories)
    {
        var childrenMap = categories
            .Where(category => category.ParentCategoryId.HasValue)
            .GroupBy(category => category.ParentCategoryId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(category => category.SortOrder).ThenBy(category => category.Name).ToList());

        CategoryNodeDto BuildNode(Category category)
        {
            var children = childrenMap.TryGetValue(category.Id, out var categoryChildren)
                ? categoryChildren.Select(categoryItem => BuildNode(categoryItem)).ToList()
                : new List<CategoryNodeDto>();

            return new CategoryNodeDto(
                category.Id,
                category.Code,
                category.Name,
                category.Scope.ToString(),
                category.IsActive,
                category.SortOrder,
                category.ParentCategoryId,
                category.UnitDefault,
                children);
        }

        return categories
            .Where(category => category.ParentCategoryId is null)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(category => BuildNode(category))
            .ToList();
    }

    private static string? ReadText(Dictionary<string, string> row, string key)
        => row.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;

    private static int? ReadInt(Dictionary<string, string> row, string key)
        => row.TryGetValue(key, out var value) && int.TryParse(value, out var parsed) ? parsed : null;

    private static bool ReadBool(Dictionary<string, string> row, string key, bool defaultValue)
    {
        if (!row.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value, out var boolValue))
        {
            return boolValue;
        }

        if (int.TryParse(value, out var intValue))
        {
            return intValue != 0;
        }

        return value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}
