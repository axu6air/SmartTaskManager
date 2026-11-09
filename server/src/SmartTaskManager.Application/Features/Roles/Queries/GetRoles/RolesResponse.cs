using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Application.Features.Roles.Queries.GetRoles;

// The permission catalogue grouped by module, and every role with its grants: everything a permission matrix needs.
public sealed record RolesResponse(
    IReadOnlyCollection<PermissionModuleResponse> Modules,
    IReadOnlyCollection<RoleResponse> Roles
);

public sealed record PermissionModuleResponse(
    string Code,
    string Name,
    IReadOnlyCollection<PermissionResponse> Permissions
);

public sealed record PermissionResponse(Guid Id, string Code, string Name, bool IsScopable);

public sealed record RoleResponse(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystem,
    IReadOnlyCollection<RoleGrantResponse> Grants
);

public sealed record RoleGrantResponse(Guid PermissionId, PermissionScopes Scope);
