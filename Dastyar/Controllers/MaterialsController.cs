using Dastyar.Application.Common;
using Dastyar.Application.Materials.Commands;
using Dastyar.Application.Materials.Dtos;
using Dastyar.Application.Materials.Queries;
using Dastyar.Domain.Entities;
using Dastyar.Filters;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/materials")]
public sealed class MaterialsController(ISender sender) : ControllerBase
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
}
