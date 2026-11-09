using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Application.Common.Abstractions;

public interface IPermissionService
{
    // The user's scopes for the permission, combined across all their roles with bitwise OR.
    // Null means the user does not hold the permission at all (or is inactive).
    Task<PermissionScopes?> GetScopesAsync(Guid userId, string permissionCode, CancellationToken ct);

    // Every permission the user holds, keyed by code, with scopes combined the same way. Empty when inactive.
    Task<IReadOnlyDictionary<string, PermissionScopes>> GetAllScopesAsync(Guid userId, CancellationToken ct);
}
