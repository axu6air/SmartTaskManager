using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Domain;

namespace SmartTaskManager.Infrastructure.Persistence;

// Stamps the audit columns on every save. Outside a request there is no current user, so the By columns
// are left as they are (the startup seeder sets them itself).
internal sealed class AuditableEntityInterceptor(ICurrentUser currentUser, TimeProvider timeProvider)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        Stamp(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        Stamp(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var now = timeProvider.GetUtcNow();
        var userId = currentUser.UserId;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry is { State: EntityState.Added, Entity: ITrackCreated created })
            {
                created.CreatedAt = now;
                created.CreatedBy = userId ?? created.CreatedBy;
            }
            else if (entry is { State: EntityState.Modified, Entity: ITrackUpdated updated })
            {
                updated.UpdatedAt = now;
                updated.UpdatedBy = userId ?? updated.UpdatedBy;
            }
        }
    }
}
