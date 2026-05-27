using Dastyar.Application.FieldDefinitions.Commands;
using Dastyar.Application.FieldDefinitions.Queries;
using Dastyar.Application.FieldDefinitions.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dastyar.Controllers;

[ApiController]
[Authorize]
[Route("api/field-definitions")]
public sealed class FieldDefinitionsController : ControllerBase
{
    private readonly ISender _sender;

    public FieldDefinitionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FieldDefinitionDto>>> List(CancellationToken cancellationToken)
    {
        var items = await _sender.Send(new GetFieldDefinitionsQuery(), cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFieldDefinitionCommand command, CancellationToken cancellationToken)
    {
        var id = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FieldDefinitionDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var item = await _sender.Send(new GetFieldDefinitionByIdQuery(id), cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFieldDefinitionRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateFieldDefinitionCommand(
            id,
            request.Name,
            request.DisplayName,
            request.DataType,
            request.IsFilterable,
            request.IsSortable,
            request.IsActive);

        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteFieldDefinitionCommand(id), cancellationToken);
        return NoContent();
    }
}
