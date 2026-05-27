/* =========================================================
 * This file is part of the Csis.Template
 * Do not make any modifications on this file
 * All changes will be rejected in code review sessions
 * ========================================================= */

using Csis.Template.Domain.Common;

namespace Csis.Template.Application.Common.Dtos;

/// <summary>
/// Base generic DTO implementation for Commands
/// </summary>
/// <typeparam name="TDto"></typeparam>
/// <typeparam name="TEntity"></typeparam>
/// <typeparam name="TKey"></typeparam>
public abstract record BaseCommandDto<TDto, TEntity, TKey> : IMappable
    where TDto : class, new()
    where TEntity : class, IEntity<TKey>, new()
{

    /// <summary>
    /// Map DTO to entity
    /// </summary>
    /// <returns></returns>
    public TEntity ToEntity() {
        return MapperProvider.Mapper.Map<TEntity>(CastToDerivedClass(this));
    }

    /// <summary>
    /// Map DTO to entity
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public TEntity ToEntity(TEntity entity) {
        return MapperProvider.Mapper.Map(CastToDerivedClass(this), entity);
    }

    /// <summary>
    /// Map entity to DTO
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static TDto FromEntity(TEntity model) {
        return MapperProvider.Mapper.Map<TDto>(model);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseInstance"></param>
    /// <returns></returns>
    protected TDto CastToDerivedClass(BaseCommandDto<TDto, TEntity, TKey> baseInstance) {
        return MapperProvider.Mapper.Map<TDto>(baseInstance);
    }

    /// <inheritdoc/>
    public void CreateMappings(Profile profile) {
        var mappingExpression = profile.CreateMap<TDto, TEntity>();

        var dtoType = typeof(TDto);
        var entityType = typeof(TEntity);
        //Ignore any property of source (like Post.Author) that dose not contains in destination 
        foreach ( var property in entityType.GetProperties() ) {
            if ( dtoType.GetProperty(property.Name) is null ) {
                mappingExpression.ForMember(property.Name, opt => opt.Ignore());
            }
        }

        ReverseCustomMappings(mappingExpression);
        CustomMappings(mappingExpression.ReverseMap());
    }

    /// <summary>
    /// Customized mapping configurations from <typeparamref name="TEntity"/> to <typeparamref name="TDto"/>
    /// </summary>
    /// <param name="mapping"></param>
    public virtual void CustomMappings(IMappingExpression<TEntity, TDto> mapping) { }

    /// <summary>
    /// Customized mapping configurations from <typeparamref name="TDto"/> to <typeparamref name="TEntity"/>
    /// </summary>
    /// <param name="mapping"></param>
    public virtual void ReverseCustomMappings(IMappingExpression<TDto, TEntity> mapping) { }
}

/// <summary>
/// Base DTO implementation for Commands
/// </summary>
/// <typeparam name="TDto"></typeparam>
/// <typeparam name="TEntity"></typeparam>
public abstract record BaseCommandDto<TDto, TEntity> : BaseCommandDto<TDto, TEntity, int>, IMappable
    where TDto : class, new()
    where TEntity : class, IEntity, new()
{

}
