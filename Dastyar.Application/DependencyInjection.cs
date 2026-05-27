using Dastyar.Application.Common.Behaviors;
using Dastyar.Application.Common.Mapping;
using Dastyar.Domain.Entities;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Dastyar.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddSingleton<IEntityTypeResolver, EntityTypeResolver>();
        services.AddScoped<INameBasedMapper, NameBasedMapper>();

        var config = TypeAdapterConfig.GlobalSettings;
        RegisterMapsterMappings(config);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }

    private static void RegisterMapsterMappings(TypeAdapterConfig config)
    {
        var domainAssembly = typeof(BaseEntity<>).Assembly;
        var applicationAssembly = typeof(EntityTypeResolver).Assembly;

        foreach (var mapping in MappingConventions.GetDtoMappings(applicationAssembly))
        {
            config.NewConfig(mapping.EntityType, mapping.DtoType);
            config.NewConfig(mapping.DtoType, mapping.EntityType);
        }

        var entities = MappingConventions.GetEntityTypes(domainAssembly).ToList();
        var namedTypes = MappingConventions.GetNamedTypes(applicationAssembly, MappingConventions.AllSuffixes)
            .Where(n => !MappingConventions.IsBaseDtoType(n.Type))
            .ToList();

        foreach (var entity in entities)
        {
            foreach (var candidate in namedTypes.Where(n =>
                         string.Equals(n.BaseName, entity.Name, StringComparison.OrdinalIgnoreCase)))
            {
                config.NewConfig(entity, candidate.Type);
                config.NewConfig(candidate.Type, entity);
            }
        }
    }
}
