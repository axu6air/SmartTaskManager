using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Domain;

namespace SmartTaskManager.Infrastructure.Repositories.Abstractions;

public abstract partial class Repository<TEntity, TContext>
{
    public virtual async Task<TEntity?> FindAsync(Guid id, CancellationToken ct = default)
    {
        return await FindAsync(id, tracking: false, ct: ct);
    }

    public virtual async Task<TEntity?> FindAsync(
        Guid id,
        bool tracking = false,
        CancellationToken ct = default
    )
    {
        return await ExecuteAsync(
                async (ct) =>
                {
                    IQueryable<TEntity> query = tracking
                        ? DbSet.AsTracking()
                        : DbSet.AsNoTracking();
                    return await query
                        .FirstOrDefaultAsync(e => e.Id == id, ct)
                        .ConfigureAwait(false);
                },
                ct
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        CancellationToken ct = default
    )
    {
        return await ExecuteAsync(
                async (ct) =>
                {
                    if (predicate is null)
                        return default;

                    var query = DbSet.Where(predicate);
                    query = tracking ? query.AsTracking() : query.AsNoTracking();

                    if (sortBy != null)
                        query = ascending ? query.OrderBy(sortBy) : query.OrderByDescending(sortBy);

                    return await query.FirstOrDefaultAsync(ct).ConfigureAwait(false);
                },
                ct
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<TEntity?> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        CancellationToken ct = default
    )
    {
        return await FindAsync(predicate, false, sortBy, ascending, ct);
    }

    public virtual async Task<ICollection<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        CancellationToken ct = default
    )
    {
        return await ExecuteAsync(
                async (ct) =>
                {
                    if (predicate is null)
                        return [];

                    var query = DbSet.Where(predicate);
                    query = tracking ? query.AsTracking() : query.AsNoTracking();

                    if (sortBy != null)
                        query = ascending ? query.OrderBy(sortBy) : query.OrderByDescending(sortBy);

                    return await query.ToListAsync(ct).ConfigureAwait(false);
                },
                ct
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<ICollection<TEntity>> FindAllAsync(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        CancellationToken ct = default
    )
    {
        return await FindAllAsync(predicate, false, sortBy, ascending, ct);
    }

    public virtual async Task<ResultSet<TEntity>> FindResultSetAsync(
        int page = 1,
        int pageSize = 30,
        Expression<Func<TEntity, bool>>? predicate = null,
        Expression<Func<TEntity, object?>>? sortBy = null,
        bool ascending = true,
        int count = 0,
        CancellationToken ct = default
    )
    {
        if (predicate is null)
            return new ResultSet<TEntity>([], count);

        var query = DbSet.Where(predicate).AsNoTracking();

        if (sortBy != null)
            query = ascending ? query.OrderBy(sortBy) : query.OrderByDescending(sortBy);

        return await FindResultSetAsync(page, pageSize, query, count, ct);
    }

    public virtual async Task<ResultSet<TEntity>> FindResultSetAsync(
        int page = 1,
        int pageSize = 30,
        IQueryable<TEntity>? query = null,
        int count = 0,
        CancellationToken ct = default
    )
    {
        return await ExecuteAsync(
            async (ct) =>
            {
                if (query is null)
                    return new ResultSet<TEntity>([], count);

                if (count == 0)
                    count = await CountAsync(query, ct);

                var entities = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                return new ResultSet<TEntity>(entities, count);
            },
            ct
        );
    }

    public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await ExecuteAsync(
            async (ct) =>
            {
                return await DbSet.AsNoTracking().AnyAsync(e => e.Id == id, ct);
            },
            ct
        );
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default
    )
    {
        return await ExecuteAsync(
                async (ct) =>
                {
                    return predicate is not null
                        && await DbSet.AsNoTracking().AnyAsync(predicate, ct).ConfigureAwait(false);
                },
                ct
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct = default
    )
    {
        return predicate is null
            ? 0
            : await ExecuteAsync(
                    async (ct) =>
                    {
                        var query = DbSet.Where(predicate).AsNoTracking();
                        return await query.CountAsync(ct).ConfigureAwait(false);
                    },
                    ct
                )
                .ConfigureAwait(false);
    }

    public virtual async Task<int> CountAsync(
        IQueryable<TEntity> query,
        CancellationToken ct = default
    )
    {
        return query is null
            ? 0
            : await ExecuteAsync(
                    async (ct) =>
                    {
                        return await query.CountAsync(ct).ConfigureAwait(false);
                    },
                    ct
                )
                .ConfigureAwait(false);
    }
}
