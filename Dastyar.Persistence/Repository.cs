using Dastyar.Application.Common;
using Dastyar.Application.Common.Interfaces;
using Dastyar.Domain.Common;
using Mapster;
using Dastyar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Dastyar.Persistence;

public sealed class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : BaseEntity<TId>, IFilterable, new()
{
    private readonly ApplicationDbContext _db;
    private readonly DbSet<TEntity> _set;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _set = db.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken)
        => await _set.FindAsync(new object?[] { id }, cancellationToken);

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations)
    {
        // مرحله: ایجاد کوئری با فیلتر حذف منطقی
        var query = ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()), navigations);
        return await query.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken, params string[] navigations)
    {
        // مرحله: ایجاد کوئری با فیلتر حذف منطقی
        var query = ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()), navigations);
        return await query.FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);
    }

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await _set.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations)
    {
        // مرحله: ایجاد کوئری با فیلتر حذف منطقی
        var query = ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()).Where(predicate), navigations);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params string[] navigations)
    {
        // مرحله: ایجاد کوئری با فیلتر حذف منطقی
        var query = ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()).Where(predicate), navigations);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken)
        => await ApplySoftDeleteFilter(_set.AsNoTracking()).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await ApplySoftDeleteFilter(_set.AsNoTracking()).Where(predicate).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(IEnumerable<SearchFilter> filters, CancellationToken cancellationToken)
        => await ApplySoftDeleteFilter(_set.AsNoTracking())
            .ApplySearchFilters(filters)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations)
    {
        // مرحله: ایجاد کوئری با فیلتر حذف منطقی
        var query = ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()), navigations);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken, params string[] navigations)
    {
        // مرحله: ایجاد کوئری با فیلتر حذف منطقی
        var query = ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()), navigations);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations)
    {
        // مرحله: ایجاد کوئری با فیلتر حذف منطقی
        var query = ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()).Where(predicate), navigations);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params string[] navigations)
    {
        // مرحله: ایجاد کوئری با فیلتر حذف منطقی
        var query = ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()).Where(predicate), navigations);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(CancellationToken cancellationToken)
        => await ApplySoftDeleteFilter(_set.AsNoTracking()).ProjectToType<TDto>().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await ApplySoftDeleteFilter(_set.AsNoTracking()).Where(predicate).ProjectToType<TDto>().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(IEnumerable<SearchFilter> filters, CancellationToken cancellationToken)
        => await ApplySoftDeleteFilter(_set.AsNoTracking())
            .ApplySearchFilters(filters)
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations)
        => await ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()), navigations)
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(CancellationToken cancellationToken, params string[] navigations)
        => await ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()), navigations)
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations)
        => await ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()).Where(predicate), navigations)
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken, params string[] navigations)
        => await ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()).Where(predicate), navigations)
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(IEnumerable<SearchFilter> filters, CancellationToken cancellationToken, params Expression<Func<TEntity, object>>[] navigations)
        => await ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()).ApplySearchFilters(filters), navigations)
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<TDto>> ListAsync<TDto>(IEnumerable<SearchFilter> filters, CancellationToken cancellationToken, params string[] navigations)
        => await ApplyIncludes(ApplySoftDeleteFilter(_set.AsNoTracking()).ApplySearchFilters(filters), navigations)
            .ProjectToType<TDto>()
            .ToListAsync(cancellationToken);

    public async Task<IPagedList<TEntity>> ListPagedAsync(BaseSearchQuery searchQuery, CancellationToken cancellationToken)
        => await ListPagedAsync(searchQuery, predicate: null, cancellationToken);

    public async Task<IPagedList<TEntity>> ListPagedAsync(BaseSearchQuery searchQuery, Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken)
    {
        // مرحله: ایجاد کوئری پایه برای صفحه بندی
        var query = ApplySoftDeleteFilter(_set.AsNoTracking());

        // مرحله: اعمال فیلترهای جستجو
        if (searchQuery.SearchFilters is { Count: > 0 })
        {
            query = query.ApplySearchFilters(searchQuery.SearchFilters);
        }

        // مرحله: اعمال شرط اضافی
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        // مرحله: اعمال مرتب سازی
        query = ApplySorting(query, searchQuery.SortBy);

        // مرحله: صفحه بندی
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((searchQuery.PageIndex - 1) * searchQuery.PageSize)
            .Take(searchQuery.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<TEntity>(items, totalCount, searchQuery.PageIndex, searchQuery.PageSize, searchQuery.SortBy);
    }

    public async Task<IPagedList<TDto>> ListPagedAsync<TDto>(BaseSearchQuery searchQuery, CancellationToken cancellationToken)
        where TDto : class
        => await ListPagedAsync<TDto>(searchQuery, predicate: null, cancellationToken);

    public async Task<IPagedList<TDto>> ListPagedAsync<TDto>(BaseSearchQuery searchQuery, Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken)
        where TDto : class
    {
        // مرحله: ایجاد کوئری پایه برای صفحه بندی
        var query = ApplySoftDeleteFilter(_set.AsNoTracking());

        // مرحله: اعمال فیلترهای جستجو
        if (searchQuery.SearchFilters is { Count: > 0 })
        {
            query = query.ApplySearchFilters(searchQuery.SearchFilters);
        }

        // مرحله: اعمال شرط اضافی
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        // مرحله: اعمال مرتب سازی
        query = ApplySorting(query, searchQuery.SortBy);

        // مرحله: صفحه بندی
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .ProjectToType<TDto>()
            .Skip((searchQuery.PageIndex - 1) * searchQuery.PageSize)
            .Take(searchQuery.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<TDto>(items, totalCount, searchQuery.PageIndex, searchQuery.PageSize, searchQuery.SortBy);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
        => await _set.AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        => await _set.AddRangeAsync(entities, cancellationToken);

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        // مرحله: بروزرسانی موجودیت
        _set.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        // مرحله: بروزرسانی گروهی موجودیت ها
        _set.UpdateRange(entities);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        // مرحله: حذف منطقی یا فیزیکی
        if (entity is ISoftDeletedEntity<TId> softDeleted)
        {
            softDeleted.SoftDelete(null, null);
            _set.Update(entity);
        }
        else
        {
            _set.Remove(entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        // مرحله: حذف منطقی یا فیزیکی گروهی
        foreach (var entity in entities)
        {
            if (entity is ISoftDeletedEntity<TId> softDeleted)
            {
                softDeleted.SoftDelete(null, null);
                _set.Update(entity);
            }
            else
            {
                _set.Remove(entity);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public void Update(TEntity entity) => _set.Update(entity);

    public void Delete(TEntity entity) => _set.Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        => _db.SaveChangesAsync(cancellationToken);

    private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, Expression<Func<TEntity, object>>[] navigations)
    {
        if (navigations is null || navigations.Length == 0)
        {
            return query;
        }

        foreach (var navigation in navigations)
        {
            if (navigation is null)
            {
                continue;
            }

            query = query.Include(navigation);
        }

        return query;
    }

    private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, string[] navigations)
    {
        if (navigations is null || navigations.Length == 0)
        {
            return query;
        }

        foreach (var navigation in navigations)
        {
            if (string.IsNullOrWhiteSpace(navigation))
            {
                continue;
            }

            query = query.Include(navigation);
        }

        return query;
    }

    private static IQueryable<TEntity> ApplySoftDeleteFilter(IQueryable<TEntity> query)
    {
        if (!typeof(ISoftDeletedEntity<TId>).IsAssignableFrom(typeof(TEntity)))
        {
            return query;
        }

        return query.Where(entity => !EF.Property<bool>(entity, nameof(ISoftDeletedEntity<TId>.Deleted)));
    }

    private static IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, string sortExpression)
    {
        if (string.IsNullOrWhiteSpace(sortExpression))
        {
            return query;
        }

        var parts = sortExpression.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var first = true;

        foreach (var part in parts)
        {
            var descending = part.StartsWith('-');
            var propertyName = part.TrimStart('-', '+');

            if (string.IsNullOrWhiteSpace(propertyName))
            {
                continue;
            }

            var property = typeof(TEntity).GetProperty(propertyName, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (property is null)
            {
                continue;
            }

            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var propertyAccess = Expression.Property(parameter, property);
            var lambda = Expression.Lambda(propertyAccess, parameter);
            var methodName = first
                ? (descending ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
                : (descending ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

            query = (IQueryable<TEntity>)typeof(Queryable)
                .GetMethods()
                .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(TEntity), property.PropertyType)
                .Invoke(null, new object[] { query, lambda })!;

            first = false;
        }

        return query;
    }
}
