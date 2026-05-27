using Dastyar.Application.Common.Interfaces;
using Dastyar.Application.FieldDefinitions.Dtos;
using Dastyar.Domain.Entities;
using MediatR;

namespace Dastyar.Application.FieldDefinitions.Queries;

public sealed record GetFieldDefinitionsQuery : IRequest<IReadOnlyList<FieldDefinitionDto>>;

public sealed class GetFieldDefinitionsQueryHandler : IRequestHandler<GetFieldDefinitionsQuery, IReadOnlyList<FieldDefinitionDto>>
{
    private readonly IRepository<FieldDefinition, Guid> _repository;

    public GetFieldDefinitionsQueryHandler(IRepository<FieldDefinition, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FieldDefinitionDto>> Handle(GetFieldDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.ListAsync(cancellationToken);
        return items.Select(x => new FieldDefinitionDto
        {
            Id = x.Id,
            Name = x.Name,
            DisplayName = x.DisplayName,
            DataType = x.DataType,
            IsFilterable = x.IsFilterable,
            IsSortable = x.IsSortable,
            IsActive = x.IsActive
        }).ToList();
    }
}
