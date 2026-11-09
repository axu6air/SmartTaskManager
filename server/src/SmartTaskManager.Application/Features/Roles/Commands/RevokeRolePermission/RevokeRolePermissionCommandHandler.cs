using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Roles.Commands.RevokeRolePermission;

public sealed class RevokeRolePermissionCommandHandler(IRoleRepository roles)
    : IRequestHandler<RevokeRolePermissionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RevokeRolePermissionCommand request, CancellationToken ct)
    {
        var role = await roles.FindWithPermissionsAsync(request.RoleId, ct);
        if (role is null)
        {
            return Result<bool>.Failure(Errors.NotFound("Role"));
        }

        if (role.IsSystem)
        {
            return Result<bool>.Failure(RoleErrors.SystemRole);
        }

        if (role.RolePermissions.All(rp => rp.PermissionId != request.PermissionId))
        {
            return Result<bool>.Failure(Errors.NotFound("Grant"));
        }

        role.Revoke(request.PermissionId);
        await roles.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }
}
