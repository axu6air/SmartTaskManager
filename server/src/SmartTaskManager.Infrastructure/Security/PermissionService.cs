using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Infrastructure.Persistence;

namespace SmartTaskManager.Infrastructure.Security;

// shortcut: one indexed query per call and no cache, add a HybridCache keyed by user ID when load needs it
// (and invalidate it whenever a user's roles or a role's grants change).
internal sealed class PermissionService(ApplicationDbContext context) : IPermissionService
{
    public async Task<PermissionScopes?> GetScopesAsync(Guid userId, string permissionCode, CancellationToken ct)
    {
        var grants = await context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.Permission.Code == permissionCode
                && context.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == rp.RoleId && ur.User.IsActive))
            .Select(rp => rp.Scope)
            .ToListAsync(ct);

        if (grants.Count == 0)
        {
            return null;
        }

        return grants.Aggregate((combined, scope) => combined | scope);
    }
}
