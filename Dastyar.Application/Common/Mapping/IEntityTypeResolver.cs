namespace Dastyar.Application.Common.Mapping;

public interface IEntityTypeResolver
{
    Type ResolveEntityType(string name);
    Type ResolveDtoType(string entityName);
}
