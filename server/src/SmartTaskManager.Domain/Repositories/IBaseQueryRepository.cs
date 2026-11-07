using System.Linq.Expressions;

namespace SmartTaskManager.Domain.Repositories;

public interface IBaseQueryRepository<TEntity> : IBaseRepository
    where TEntity : Entity
{
    Task<TEntity?> FindAsync(Guid id, CancellationToken ct = default);

    Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        CancellationToken ct = default
    );

    Task<ICollection<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        CancellationToken ct = default
    );

    Task<ResultSet<TEntity>> FindResultSetAsync(
        int page = 1,
        int pageSize = 30,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        int count = 0,
        CancellationToken ct = default
    );

    Task<ResultSet<TEntity>> FindResultSetAsync(
        int page = 1,
        int pageSize = 30,
        IQueryable<TEntity>? query = null,
        int count = 0,
        CancellationToken ct = default
    );

    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default
    );

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    Task<int> CountAsync(IQueryable<TEntity> query, CancellationToken ct = default);
}
