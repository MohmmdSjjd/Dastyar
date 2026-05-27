using Dastyar.Application.Products.Commands;
using Dastyar.Application.Products.Dtos;
using Dastyar.Application.Products.Queries;
using Dastyar.Domain.Entities;
using Dastyar.Filters;
using Dastyar.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/products")]
public sealed class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetProducts(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetProductsQuery(), cancellationToken);
        return Ok(items);
    }

    [HttpPost("search")]
    [DynamicSearchAttribute<Product>]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> SearchProducts([FromBody] GetProductsQuery query, CancellationToken cancellationToken)
    {
        var items = await _sender.Send(query, cancellationToken);
        return Ok(items);
    }

    [HttpGet("search/simple")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> SimpleSearch([FromQuery] string q, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<ProductDto>());
        }

        // Get all products and filter by code, name, or any dynamic field (case-insensitive)
        var allProducts = await _sender.Send(new GetProductsQuery(), cancellationToken);
        var searchTerm = q.ToLower();
        var filtered = allProducts
            .Where(p =>
                (p.Code?.ToLower() ?? "").Contains(searchTerm) ||
                (p.Name?.ToLower() ?? "").Contains(searchTerm) ||
                SearchInDynamicFields(p.DynamicFieldsJson, searchTerm)
            )
            .ToList();

        return Ok(filtered);
    }

    private static bool SearchInDynamicFields(string? dynamicFieldsJson, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(dynamicFieldsJson))
            return false;

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
                        return true;
                }
            }
        }
        catch
        {
            // If parsing fails, skip this product
            return false;
        }

        return false;
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateProduct([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _sender.Send(command, cancellationToken);
            return Created($"/api/products/{id}", new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("IX_Products_Code") == true)
            {
                return BadRequest(new { error = "محصول با این کد قبلاً وجود دارد." });
            }
            return StatusCode(500, new { error = "خطا در ایجاد محصول" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "خطا در ایجاد محصول" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateProductCommand(id, request.Code, request.Name, request.IsActive, request.DynamicFieldsJson);
            await _sender.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            if (ex.InnerException?.Message.Contains("IX_Products_Code") == true)
            {
                return BadRequest(new { error = "محصول با این کد قبلاً وجود دارد." });
            }
            return StatusCode(500, new { error = "خطا در ویرایش محصول" });
        }
        catch (Exception ex)
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

            var result = await _sender.Send(new ImportProductsCommand(command.File));
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "خطا در ایمپورت محصولات" });
        }
    }

    [HttpPost("export")]
    public async Task<IActionResult> ExportProducts([FromBody] ExportProductsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sender.Send(new ExportProductsQuery(request?.Fields), cancellationToken);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "خطا در خروجی‌گیری" });
        }
    }
}
