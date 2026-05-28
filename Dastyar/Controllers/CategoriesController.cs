using Dastyar.Application.Categories.Dtos;
using Dastyar.Application.Categories.Commands;
using Dastyar.Application.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ISender _sender;

    public CategoriesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] string? scope, CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetCategoriesQuery(scope), cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var id = await _sender.Send(command, cancellationToken);
            return Created($"/api/categories/{id}", new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(
                new UpdateCategoryCommand(
                    id,
                    request.Code,
                    request.Name,
                    request.ParentCategoryCode,
                    request.UnitDefault,
                    request.Scope,
                    request.IsActive,
                    request.SortOrder),
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _sender.Send(new DeleteCategoryCommand(id), cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
