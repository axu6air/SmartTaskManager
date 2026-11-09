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
        var grants = await GrantsOf(userId)
            .Where(rp => rp.Permission.Code == permissionCode)
            .Select(rp => rp.Scope)
            .ToListAsync(ct);

        if (grants.Count == 0)
        {
            return null;
        }

        return grants.Aggregate((combined, scope) => combined | scope);
    }

    public async Task<IReadOnlyDictionary<string, PermissionScopes>> GetAllScopesAsync(Guid userId, CancellationToken ct)
    {
        var grants = await GrantsOf(userId)
            .Select(rp => new { rp.Permission.Code, rp.Scope })
            .ToListAsync(ct);

        // Combined in memory: SQL has no portable bitwise-OR aggregate, and a user holds only a few dozen grants.
        return grants
            .GroupBy(g => g.Code)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Scope).Aggregate((combined, scope) => combined | scope));
    }

    private IQueryable<RolePermission> GrantsOf(Guid userId) =>
        context.RolePermissions
            .AsNoTracking()
            .Where(rp => context.UserRoles.Any(ur => ur.UserId == userId && ur.RoleId == rp.RoleId && ur.User.IsActive));
}
