using System.Linq.Expressions;

namespace SmartTaskManager.Domain.Repositories;

public interface IBaseCommandRepository<TEntity> : IBaseRepository
    where TEntity : Entity
{
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <inheritdoc cref="DeleteAsync(Guid, CancellationToken)"/>
    /// <param name="saveChanges">
    /// When false the change is only staged on the change tracker; call
    /// <see cref="SaveChangesAsync"/> to persist it.
    /// </param>
    Task DeleteAsync(Guid id, bool saveChanges, CancellationToken ct = default);

    Task DeleteAsync(TEntity entity, CancellationToken ct = default);

    /// <inheritdoc cref="DeleteAsync(Guid, bool, CancellationToken)"/>
    Task DeleteAsync(TEntity entity, bool saveChanges, CancellationToken ct = default);

    Task DeleteAllAsync(ICollection<TEntity> entities, CancellationToken ct = default);

    /// <inheritdoc cref="DeleteAsync(Guid, bool, CancellationToken)"/>
    Task DeleteAllAsync(
        ICollection<TEntity> entities,
        bool saveChanges,
        CancellationToken ct = default
    );

    Task<TEntity?> CreateAsync(TEntity entity, CancellationToken ct = default);

    /// <inheritdoc cref="DeleteAsync(Guid, bool, CancellationToken)"/>
    Task<TEntity?> CreateAsync(TEntity entity, bool saveChanges, CancellationToken ct = default);

    Task CreateAllAsync(IEnumerable<TEntity> entities, CancellationToken ct = default);

    /// <inheritdoc cref="DeleteAsync(Guid, bool, CancellationToken)"/>
    Task CreateAllAsync(
        IEnumerable<TEntity> entities,
        bool saveChanges,
        CancellationToken ct = default
    );

    Task UpdateAsync(TEntity entity, CancellationToken ct = default);

    /// <inheritdoc cref="DeleteAsync(Guid, bool, CancellationToken)"/>
    Task UpdateAsync(TEntity entity, bool saveChanges, CancellationToken ct = default);

    Task UpdateAsync(Guid id, Func<TEntity, TEntity> updateFunc, CancellationToken ct = default);

    /// <inheritdoc cref="DeleteAsync(Guid, bool, CancellationToken)"/>
    Task UpdateAsync(
        Guid id,
        Func<TEntity, TEntity> updateFunc,
        bool saveChanges,
        CancellationToken ct = default
    );

    Task UpdateAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<TEntity> updateAction,
        CancellationToken ct = default
    );

    /// <inheritdoc cref="DeleteAsync(Guid, bool, CancellationToken)"/>
    Task UpdateAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<TEntity> updateAction,
        bool saveChanges,
        CancellationToken ct = default
    );

    /// <summary>
    /// Persists the changes staged by command overloads called with <c>saveChanges: false</c>.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
