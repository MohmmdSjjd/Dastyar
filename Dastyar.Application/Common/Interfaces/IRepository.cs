using Dastyar.Application.Common;
using Dastyar.Domain.Common;
using Dastyar.Domain.Entities;
using System.Linq.Expressions;

namespace Dastyar.Application.Common.Interfaces;

public interface IRepository<TEntity, TId> where TEntity : BaseEntity<TId>, IFilterable, new()
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken);
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations);
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken, params string[] navigations);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations);
    Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params string[] navigations);
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<IReadOnlyList<TEntity>> ListAsync(IEnumerable<SearchFilter> filters, CancellationToken cancellationToken);
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations);
    Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken, params string[] navigations);
    Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations);
    Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params string[] navigations);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(CancellationToken cancellationToken);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(IEnumerable<SearchFilter> filters, CancellationToken cancellationToken);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(CancellationToken cancellationToken, params string[] navigations);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params string[] navigations);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(IEnumerable<SearchFilter> filters, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations);
    Task<IReadOnlyList<TDto>> ListAsync<TDto>(IEnumerable<SearchFilter> filters, CancellationToken cancellationToken, params string[] navigations);
    Task<IPagedList<TEntity>> ListPagedAsync(BaseSearchQuery searchQuery, CancellationToken cancellationToken);
    Task<IPagedList<TEntity>> ListPagedAsync(BaseSearchQuery searchQuery, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<IPagedList<TDto>> ListPagedAsync<TDto>(BaseSearchQuery searchQuery, CancellationToken cancellationToken) where TDto : class;
    Task<IPagedList<TDto>> ListPagedAsync<TDto>(BaseSearchQuery searchQuery, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken) where TDto : class;
    Task AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken);
    Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken);
    Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
