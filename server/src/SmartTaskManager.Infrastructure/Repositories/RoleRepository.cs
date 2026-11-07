using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class RoleRepository(ApplicationDbContext context)
    : Repository<Role, ApplicationDbContext>(context),
        IRoleRepository
{
    public Task<Role?> FindWithPermissionsAsync(Guid id, CancellationToken ct = default)
    {
        return ExecuteAsync(
            (ct) =>
                DbSet
                    .AsTracking()
                    .Include(r => r.RolePermissions)
                    .FirstOrDefaultAsync(r => r.Id == id, ct),
            ct
        );
    }
}
