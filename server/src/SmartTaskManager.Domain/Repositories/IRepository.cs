using System.Data;
using System.Linq.Expressions;

namespace SmartTaskManager.Domain.Repositories;

public interface IRepository<TEntity>
    : IBaseCommandRepository<TEntity>,
        IBaseQueryRepository<TEntity>
    where TEntity : Entity
{
    Task<TEntity?> FindAsync(Guid id, bool tracking = false, CancellationToken ct = default);

    Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        CancellationToken ct = default
    );

    Task<ICollection<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        CancellationToken ct = default
    );

    Task ExecuteTransactionAsync(
        Func<Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default
    );

    Task<T> ExecuteTransactionAsync<T>(
        Func<Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default
    );

    Task<ICollection<TResult>> ExecuteStoredProcedureAsync<TResult>(
        string storedProcedure,
        CancellationToken ct = default,
        params object[] parameters
    )
        where TResult : class;
}
