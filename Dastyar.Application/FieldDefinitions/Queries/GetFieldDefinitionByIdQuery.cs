using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.FieldDefinitions.Dtos;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.FieldDefinitions.Queries;

public sealed record GetFieldDefinitionByIdQuery(Guid Id) : IRequest<FieldDefinitionDto?>;

public sealed class GetFieldDefinitionByIdQueryHandler : IRequestHandler<GetFieldDefinitionByIdQuery, FieldDefinitionDto?>
{
    private readonly IRepository<FieldDefinition, Guid> _repository;

    public GetFieldDefinitionByIdQueryHandler(IRepository<FieldDefinition, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<FieldDefinitionDto?> Handle(GetFieldDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null) return null;

        return new FieldDefinitionDto
        {
            Id = item.Id,
            Name = item.Name,
            DisplayName = item.DisplayName,
            DataType = item.DataType,
            IsFilterable = item.IsFilterable,
            IsSortable = item.IsSortable,
            IsActive = item.IsActive
        };
    }
}
