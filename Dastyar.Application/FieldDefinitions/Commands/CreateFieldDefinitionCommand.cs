using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.FieldDefinitions.Commands;

public sealed record CreateFieldDefinitionCommand(
    string Name,
    string? DisplayName,
    string DataType,
    bool IsFilterable,
    bool IsSortable,
    bool IsActive) : IRequest<Guid>;

public sealed class CreateFieldDefinitionCommandHandler : IRequestHandler<CreateFieldDefinitionCommand, Guid>
{
    private readonly IRepository<FieldDefinition, Guid> _repository;

    public CreateFieldDefinitionCommandHandler(IRepository<FieldDefinition, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = new FieldDefinition
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Name.Trim() : request.DisplayName.Trim(),
            DataType = string.IsNullOrWhiteSpace(request.DataType) ? "string" : request.DataType.Trim(),
            IsFilterable = request.IsFilterable,
            IsSortable = request.IsSortable,
            IsActive = request.IsActive
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
