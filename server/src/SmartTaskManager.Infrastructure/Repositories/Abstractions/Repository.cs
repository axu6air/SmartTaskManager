using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Domain;
using SmartTaskManager.Infrastructure.Errors;
using SmartTaskManager.Infrastructure.Exceptions;

namespace SmartTaskManager.Infrastructure.Repositories.Abstractions;

public abstract partial class Repository<TEntity, TContext> : IEfRepository<TEntity, TContext>
    where TEntity : Entity
    where TContext : DbContext
{
    public TContext Context { get; private set; }
    public DbSet<TEntity> DbSet { get; private set; }

    protected Repository(TContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = Context.Set<TEntity>();
    }

    public virtual async Task ExecuteTransactionAsync(
        Func<Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default
    )
    {
        await ExecuteTransactionAsync(
            async () =>
            {
                await action();
                return true;
            },
            isolationLevel,
            ct
        );
    }

    public virtual async Task<T> ExecuteTransactionAsync<T>(
        Func<Task<T>> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken ct = default
    )
    {
        return await ExecuteAsync(
                async (ct) =>
                {
                    await using var transaction = await Context.Database.BeginTransactionAsync(
                        isolationLevel,
                        ct
                    );

                    var result = await action().ConfigureAwait(false);
                    await Context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                    return result;
                },
                ct
            )
            .ConfigureAwait(false);
    }

    public virtual async Task<ICollection<TResult>> ExecuteStoredProcedureAsync<TResult>(
        string storedProcedure,
        CancellationToken ct = default,
        params object[] parameters
    )
        where TResult : class
    {
        return await ExecuteAsync(
            async (ct) =>
            {
                return await Context
                    .Set<TResult>()
                    .FromSqlRaw(storedProcedure, parameters)
                    .ToListAsync(ct);
            },
            ct
        );
    }

    protected static async Task ExecuteAsync(
        Func<CancellationToken, Task> action,
        CancellationToken ct = default
    )
    {
        await ExecuteAsync(
            async ct =>
            {
                await action(ct);
                return true;
            },
            ct
        );
    }

    protected static async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct = default
    )
    {
        try
        {
            ct.ThrowIfCancellationRequested();

            var result = await action(ct);
            return result;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (SqlException ex)
        {
            throw MapSqlException(ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new RepositoryException(ex, "Concurrency conflict: {0}", ex.Message)
            {
                ErrorType = RepositoryErrorType.ConcurrencyConflict,
                IsRetriable = true,
            };
        }
        catch (DbUpdateException ex)
        {
            throw new RepositoryException(ex, "Database update failed: {0}", ex.Message)
            {
                ErrorType = RepositoryErrorType.UpdateFailed,
            };
        }
        catch (TimeoutException ex)
        {
            throw new RepositoryException(ex, "Operation timeout: {0}", ex.Message)
            {
                ErrorType = RepositoryErrorType.Timeout,
            };
        }
        catch (RepositoryException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new RepositoryException(ex, ex.Message)
            {
                ErrorType = RepositoryErrorType.Unknown,
            };
        }
    }

    private static RepositoryException MapSqlException(SqlException ex)
    {
        if (ex.Number == SqlErrorCodes.Timeout)
        {
            return new RepositoryException(ex, "Database query timeout: {0}", ex.Message)
            {
                ErrorType = RepositoryErrorType.CommandTimeout,
            };
        }
        else if (
            ex.Number == SqlErrorCodes.ConnectionTimeout
            || ex.Number == SqlErrorCodes.ConnectionBroken
        )
        {
            return new RepositoryException(ex, "Database connection timeout: {0}", ex.Message)
            {
                ErrorType = RepositoryErrorType.ConnectionTimeout,
            };
        }
        else if (ex.Number == SqlErrorCodes.Deadlock)
        {
            return new RepositoryException(ex, "Database deadlock occurred: {0}", ex.Message)
            {
                ErrorType = RepositoryErrorType.Deadlock,
                IsRetriable = true,
            };
        }

        return new RepositoryException(ex, "SQL error: {0}", ex.Message)
        {
            ErrorType = RepositoryErrorType.SqlError,
        };
    }
}
