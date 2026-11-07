using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SmartTaskManager.Domain;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Infrastructure.Repositories.Abstractions;

public interface IEfRepository<TEntity, TContext> : IRepository<TEntity>
    where TEntity : Entity
    where TContext : DbContext
{
    DbSet<TEntity> DbSet { get; }
    TContext Context { get; }

    /// <summary>
    /// Bulk-updates every row matching <paramref name="predicate"/> with a single SQL statement.
    /// </summary>
    /// <remarks>
    /// Executes immediately and bypasses the change tracker, so it does not stage anything for
    /// <see cref="IBaseCommandRepository{TEntity}.SaveChangesAsync"/> and does not run
    /// <c>SaveChanges</c> interceptors or concurrency checks.
    /// </remarks>
    Task UpdateAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<UpdateSettersBuilder<TEntity>> setPropertyCalls,
        CancellationToken ct = default
    );

    /// <summary>
    /// Bulk-deletes every row matching <paramref name="predicate"/> with a single SQL statement.
    /// </summary>
    /// <inheritdoc cref="UpdateAllAsync(Expression{Func{TEntity, bool}}, Action{UpdateSettersBuilder{TEntity}}, CancellationToken)" path="/remarks"/>
    Task DeleteAllAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
}
