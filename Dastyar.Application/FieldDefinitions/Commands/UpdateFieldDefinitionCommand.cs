using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.FieldDefinitions.Commands;

public sealed record UpdateFieldDefinitionCommand(
    Guid Id,
    string Name,
    string? DisplayName,
    string DataType,
    bool IsFilterable,
    bool IsSortable,
    bool IsActive) : IRequest;

public sealed class UpdateFieldDefinitionCommandHandler : IRequestHandler<UpdateFieldDefinitionCommand>
{
    private readonly IRepository<FieldDefinition, Guid> _repository;

    public UpdateFieldDefinitionCommandHandler(IRepository<FieldDefinition, Guid> repository)
    {
        _repository = repository;
    }

    public async Task Handle(UpdateFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity is null)
        {
            throw new InvalidOperationException("FieldDefinition not found.");
        }

        entity.Name = request.Name.Trim();
        entity.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Name.Trim() : request.DisplayName.Trim();
        entity.DataType = string.IsNullOrWhiteSpace(request.DataType) ? "string" : request.DataType.Trim();
        entity.IsFilterable = request.IsFilterable;
        entity.IsSortable = request.IsSortable;
        entity.IsActive = request.IsActive;

        _repository.Update(entity);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
