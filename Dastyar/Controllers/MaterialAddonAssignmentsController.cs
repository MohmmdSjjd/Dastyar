using Dastyar.Application.Materials.Commands;
using Dastyar.Application.Materials.Dtos;
using Dastyar.Application.Materials.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/material-addon-assignments")]
public sealed class MaterialAddonAssignmentsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MaterialAddonAssignmentDto>>> List(CancellationToken cancellationToken)
    {
        var items = await sender.Send(new GetMaterialAddonAssignmentsQuery(), cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialAddonAssignmentCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var id = await sender.Send(command, cancellationToken);
            return Created($"/api/material-addon-assignments/{id}", new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaterialAddonAssignmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(
                new UpdateMaterialAddonAssignmentCommand(
                    id,
                    request.AddonMaterialCode,
                    request.TargetCategoryCode,
                    request.TargetMaterialCode,
                    request.Quantity,
                    request.UnitOverride),
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await sender.Send(new DeleteMaterialAddonAssignmentCommand(id), cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
