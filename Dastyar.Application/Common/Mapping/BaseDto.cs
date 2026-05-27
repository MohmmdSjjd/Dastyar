using Dastyar.Domain.Entities;

namespace Dastyar.Application.Common.Mapping;

public abstract record BaseDto<TDto, TEntity, TId>
    where TDto : BaseDto<TDto, TEntity, TId>
    where TEntity : BaseEntity<TId>
{
    public TId Id { get; init; } = default!;
}

public abstract record BaseCommandDto<TDto, TEntity, TId> : BaseDto<TDto, TEntity, TId>
    where TDto : BaseCommandDto<TDto, TEntity, TId>
    where TEntity : BaseEntity<TId>;
