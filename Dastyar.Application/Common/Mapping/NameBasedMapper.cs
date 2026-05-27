using MapsterMapper;

namespace Dastyar.Application.Common.Mapping;

public sealed class NameBasedMapper : INameBasedMapper
{
    private readonly IMapper _mapper;
    private readonly IEntityTypeResolver _resolver;

    public NameBasedMapper(IMapper mapper, IEntityTypeResolver resolver)
    {
        _mapper = mapper;
        _resolver = resolver;
    }

    public object MapToEntity(string entityName, object source)
    {
        var entityType = _resolver.ResolveEntityType(entityName);
        return _mapper.Map(source, source.GetType(), entityType);
    }

    public object MapToDto(string entityName, object source)
    {
        var dtoType = _resolver.ResolveDtoType(entityName);
        return _mapper.Map(source, source.GetType(), dtoType);
    }
}
