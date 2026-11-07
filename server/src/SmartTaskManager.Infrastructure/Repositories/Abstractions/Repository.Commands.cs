using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SmartTaskManager.Domain;

namespace SmartTaskManager.Infrastructure.Repositories.Abstractions;

public abstract partial class Repository<TEntity, TContext>
{
    public virtual Task<TEntity?> CreateAsync(TEntity entity, CancellationToken ct = default)
    {
        return CreateAsync(entity, saveChanges: true, ct);
    }

    public virtual async Task<TEntity?> CreateAsync(
        TEntity entity,
        bool saveChanges,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity is ITrackCreated trackableEntity)
        {
            trackableEntity.CreatedAt = DateTime.UtcNow;
        }

        return await ExecuteAsync(
            async (ct) =>
            {
                await DbSet.AddAsync(entity, ct).ConfigureAwait(false);

                if (saveChanges)
                    await Context.SaveChangesAsync(ct).ConfigureAwait(false);

                return entity;
            },
            ct
        );
    }

    public virtual Task CreateAllAsync(
        IEnumerable<TEntity> entities,
        CancellationToken ct = default
    )
    {
        return CreateAllAsync(entities, saveChanges: true, ct);
    }

    public virtual async Task CreateAllAsync(
        IEnumerable<TEntity> entities,
        bool saveChanges,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);

        await ExecuteAsync(
                async (ct) =>
                {
                    var now = DateTime.UtcNow;

                    foreach (var entity in entities)
                    {
                        if (entity is ITrackCreated trackableEntity)
                        {
                            trackableEntity.CreatedAt = now;
                        }
                    }

                    await DbSet.AddRangeAsync(entities, ct).ConfigureAwait(false);

                    if (saveChanges)
                        await Context.SaveChangesAsync(ct).ConfigureAwait(false);
                },
                ct
            )
            .ConfigureAwait(false);
    }

    public virtual Task UpdateAsync(
        Guid id,
        Func<TEntity, TEntity> updateFunc,
        CancellationToken ct = default
    )
    {
        return UpdateAsync(id, updateFunc, saveChanges: true, ct);
    }

    public virtual async Task UpdateAsync(
        Guid id,
        Func<TEntity, TEntity> updateFunc,
        bool saveChanges,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(updateFunc);

        var entity = await FindAsync(id, true, ct);

        if (entity is null)
            return;

        entity = updateFunc(entity);

        if (entity is ITrackUpdated trackableEntity)
        {
            trackableEntity.UpdatedAt = DateTime.UtcNow;
        }

        await ExecuteAsync(
            async (ct) =>
            {
                DbSet.Update(entity);

                if (saveChanges)
                    await Context.SaveChangesAsync(ct).ConfigureAwait(false);
            },
            ct
        );
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken ct = default)
    {
        return UpdateAsync(entity, saveChanges: true, ct);
    }

    public virtual async Task UpdateAsync(
        TEntity entity,
        bool saveChanges,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity is ITrackUpdated trackableEntity)
        {
            trackableEntity.UpdatedAt = DateTime.UtcNow;
        }

        await ExecuteAsync(
            async (ct) =>
            {
                DbSet.Update(entity);

                if (saveChanges)
                    await Context.SaveChangesAsync(ct).ConfigureAwait(false);
            },
            ct
        );
    }

    public virtual async Task UpdateAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<UpdateSettersBuilder<TEntity>> setPropertyCalls,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);

        await ExecuteAsync(
            async (ct) =>
            {
                await DbSet
                    .Where(predicate)
                    .ExecuteUpdateAsync(setPropertyCalls, ct)
                    .ConfigureAwait(false);
            },
            ct
        );
    }

    public virtual Task UpdateAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<TEntity> updateAction,
        CancellationToken ct = default
    )
    {
        return UpdateAllAsync(predicate, updateAction, saveChanges: true, ct);
    }

    public virtual async Task UpdateAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Action<TEntity> updateAction,
        bool saveChanges,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(updateAction);

        await ExecuteAsync(
                async (ct) =>
                {
                    var now = DateTime.UtcNow;

                    var entities = await DbSet
                        .Where(predicate)
                        .ToListAsync(ct)
                        .ConfigureAwait(false);

                    foreach (var entity in entities)
                    {
                        updateAction(entity);

                        if (entity is ITrackUpdated trackableEntity)
                        {
                            trackableEntity.UpdatedAt = now;
                        }
                    }

                    DbSet.UpdateRange(entities);

                    if (saveChanges)
                        await Context.SaveChangesAsync(ct).ConfigureAwait(false);
                },
                ct
            )
            .ConfigureAwait(false);
    }

    public virtual Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        return DeleteAsync(id, saveChanges: true, ct);
    }

    public virtual Task DeleteAsync(Guid id, bool saveChanges, CancellationToken ct = default)
    {
        return ExecuteAsync(
            async (ct) =>
            {
                var entity = await DbSet.FindAsync([id], ct).ConfigureAwait(false);

                if (entity is null)
                    return;

                DbSet.Remove(entity);

                if (saveChanges)
                    await Context.SaveChangesAsync(ct).ConfigureAwait(false);
            },
            ct
        );
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken ct = default)
    {
        return DeleteAsync(entity, saveChanges: true, ct);
    }

    public virtual Task DeleteAsync(
        TEntity entity,
        bool saveChanges,
        CancellationToken ct = default
    )
    {
        return ExecuteAsync(
            async (ct) =>
            {
                DbSet.Remove(entity);

                if (saveChanges)
                    await Context.SaveChangesAsync(ct).ConfigureAwait(false);
            },
            ct
        );
    }

    public virtual Task DeleteAllAsync(
        ICollection<TEntity> entities,
        CancellationToken ct = default
    )
    {
        return DeleteAllAsync(entities, saveChanges: true, ct);
    }

    public virtual async Task DeleteAllAsync(
        ICollection<TEntity> entities,
        bool saveChanges,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(entities);

        await ExecuteAsync(
            async (ct) =>
            {
                DbSet.RemoveRange(entities);

                if (saveChanges)
                    await Context.SaveChangesAsync(ct).ConfigureAwait(false);
            },
            ct
        );
    }

    public virtual async Task DeleteAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(predicate);

        await ExecuteAsync(
            async (ct) =>
            {
                await DbSet.Where(predicate).ExecuteDeleteAsync(ct).ConfigureAwait(false);
            },
            ct
        );
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await ExecuteAsync(Context.SaveChangesAsync, ct).ConfigureAwait(false);
    }
}
