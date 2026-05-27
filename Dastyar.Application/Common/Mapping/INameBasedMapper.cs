namespace Dastyar.Application.Common.Mapping;

public interface INameBasedMapper
{
    object MapToEntity(string entityName, object source);
    object MapToDto(string entityName, object source);
}
