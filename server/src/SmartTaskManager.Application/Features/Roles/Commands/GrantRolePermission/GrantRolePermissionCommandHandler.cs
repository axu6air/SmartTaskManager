using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Roles.Queries.GetRoles;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Roles.Commands.GrantRolePermission;

public sealed class GrantRolePermissionCommandHandler(IRoleRepository roles, IPermissionRepository permissions)
    : IRequestHandler<GrantRolePermissionCommand, Result<RoleGrantResponse>>
{
    public async Task<Result<RoleGrantResponse>> Handle(GrantRolePermissionCommand request, CancellationToken ct)
    {
        var role = await roles.FindWithPermissionsAsync(request.RoleId, ct);
        if (role is null)
        {
            return Result<RoleGrantResponse>.Failure(Errors.NotFound("Role"));
        }

        if (role.IsSystem)
        {
            return Result<RoleGrantResponse>.Failure(RoleErrors.SystemRole);
        }

        var permission = await permissions.FindAsync(request.PermissionId, tracking: true, ct: ct);
        if (permission is null)
        {
            return Result<RoleGrantResponse>.Failure(Errors.NotFound("Permission"));
        }

        if (!permission.IsScopable && request.Scope != PermissionScopes.All)
        {
            return Result<RoleGrantResponse>.Failure(RoleErrors.UnscopedNeedsAll);
        }

        role.Grant(permission, request.Scope);
        await roles.SaveChangesAsync(ct);

        return Result<RoleGrantResponse>.Success(new RoleGrantResponse(permission.Id, request.Scope));
    }
}
