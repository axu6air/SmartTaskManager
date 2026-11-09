using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Roles.Queries.GetRoles;

public sealed class GetRolesQueryHandler(
    IPermissionModuleRepository modules,
    IPermissionRepository permissions,
    IRoleRepository roles
) : IRequestHandler<GetRolesQuery, Result<RolesResponse>>
{
    public async Task<Result<RolesResponse>> Handle(GetRolesQuery request, CancellationToken ct)
    {
        var allModules = await modules.FindAllAsync(_ => true, sortBy: m => m.DisplayOrder, ct: ct);
        var allPermissions = await permissions.FindAllAsync(_ => true, sortBy: p => p.DisplayOrder, ct: ct);
        var allRoles = await roles.FindAllWithPermissionsAsync(ct);

        var moduleResponses = allModules
            .Select(m => new PermissionModuleResponse(
                m.Code,
                m.Name,
                allPermissions
                    .Where(p => p.ModuleId == m.Id)
                    .Select(p => new PermissionResponse(p.Id, p.Code, p.Name, p.IsScopable))
                    .ToList()
            ))
            .ToList();

        var roleResponses = allRoles
            .Select(r => new RoleResponse(
                r.Id,
                r.Name,
                r.Description,
                r.IsSystem,
                r.RolePermissions.Select(rp => new RoleGrantResponse(rp.PermissionId, rp.Scope)).ToList()
            ))
            .ToList();

        return Result<RolesResponse>.Success(new RolesResponse(moduleResponses, roleResponses));
    }
}
