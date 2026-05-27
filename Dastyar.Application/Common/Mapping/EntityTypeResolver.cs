using Dastyar.Domain.Entities;

namespace Dastyar.Application.Common.Mapping;

public sealed class EntityTypeResolver : IEntityTypeResolver
{
    private readonly IReadOnlyDictionary<string, Type> _entityTypes;
    private readonly IReadOnlyDictionary<string, Type> _dtoTypesByEntity;
    private readonly IReadOnlyDictionary<string, Type> _dtoTypesByName;

    public EntityTypeResolver()
    {
        var domainAssembly = typeof(BaseEntity<>).Assembly;
        var applicationAssembly = typeof(EntityTypeResolver).Assembly;

        _entityTypes = MappingConventions.GetEntityTypes(domainAssembly)
            .ToDictionary(t => MappingConventions.Normalize(t.Name), t => t);

        var dtoByEntity = new Dictionary<string, Type>();
        var dtoByName = new Dictionary<string, Type>();

        foreach (var mapping in MappingConventions.GetDtoMappings(applicationAssembly))
        {
            var entityKey = MappingConventions.Normalize(mapping.EntityType.Name);
            dtoByEntity.TryAdd(entityKey, mapping.DtoType);

            var dtoKey = MappingConventions.Normalize(MappingConventions.StripSuffix(mapping.DtoType.Name));
            dtoByName.TryAdd(dtoKey, mapping.DtoType);
        }

        foreach (var named in MappingConventions.GetNamedTypes(applicationAssembly, MappingConventions.DtoSuffixes)
                     .Where(n => !MappingConventions.IsBaseDtoType(n.Type)))
        {
            var dtoKey = MappingConventions.Normalize(named.BaseName);
            dtoByName.TryAdd(dtoKey, named.Type);
        }

        _dtoTypesByEntity = dtoByEntity;
        _dtoTypesByName = dtoByName;
    }

    public Type ResolveEntityType(string name)
    {
        var normalized = MappingConventions.Normalize(MappingConventions.StripSuffix(name));

        if (!_entityTypes.TryGetValue(normalized, out var entityType))
        {
            throw new KeyNotFoundException($"Entity '{name}' not found.");
        }

        return entityType;
    }

    public Type ResolveDtoType(string entityName)
    {
        var normalized = MappingConventions.Normalize(MappingConventions.StripSuffix(entityName));

        if (_dtoTypesByEntity.TryGetValue(normalized, out var dtoType))
        {
            return dtoType;
        }

        if (_dtoTypesByName.TryGetValue(normalized, out dtoType))
        {
            return dtoType;
        }

        throw new KeyNotFoundException($"DTO for entity '{entityName}' not found.");
    }
}
