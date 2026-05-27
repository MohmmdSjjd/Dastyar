using Dastyar.Application.Common.Mapping;
using Dastyar.Domain.Entities;

namespace Dastyar.Application.FieldDefinitions.Dtos;

public sealed record FieldDefinitionDto : BaseDto<FieldDefinitionDto, FieldDefinition, Guid>
{
    public string Name { get; init; } = string.Empty;
    public string? DisplayName { get; init; }
    public string DataType { get; init; } = "string";
    public bool IsFilterable { get; init; }
    public bool IsSortable { get; init; }
    public bool IsActive { get; init; }
}
