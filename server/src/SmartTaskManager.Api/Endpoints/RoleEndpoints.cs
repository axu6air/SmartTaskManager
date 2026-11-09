using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Roles.Commands.GrantRolePermission;
using SmartTaskManager.Application.Features.Roles.Commands.RevokeRolePermission;
using SmartTaskManager.Application.Features.Roles.Queries.GetRoles;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Api.Endpoints;

// Scope is a flags number: Assigned 1, Managed 2, Member 4, All 8.
public sealed record GrantRolePermissionRequest(PermissionScopes Scope);

public static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles").WithTags("Roles");

        group.MapGet(
                "/",
                async (HttpContext http, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new GetRolesQuery(), ct)).ToHttpResult(http)
            )
            .WithName("GetRoles")
            .WithSummary("Lists the permission catalogue and every role with its grants.")
            .Produces<ApiResponse<RolesResponse>>()
            .ProducesErrors(StatusCodes.Status401Unauthorized, StatusCodes.Status403Forbidden)
            .Authorize(Permissions.Roles.View);

        group.MapPut(
                "/{roleId:guid}/permissions/{permissionId:guid}",
                async (
                    Guid roleId,
                    Guid permissionId,
                    GrantRolePermissionRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                    (await sender.Send(new GrantRolePermissionCommand(roleId, permissionId, request.Scope), ct))
                        .ToHttpResult(http)
            )
            .WithName("GrantRolePermission")
            .WithSummary("Grants a permission to a role, or changes the scope of an existing grant.")
            .Produces<ApiResponse<RoleGrantResponse>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict
            )
            .Authorize(Permissions.Roles.Manage);

        group.MapDelete(
                "/{roleId:guid}/permissions/{permissionId:guid}",
                async (Guid roleId, Guid permissionId, HttpContext http, ISender sender, CancellationToken ct) =>
                    (await sender.Send(new RevokeRolePermissionCommand(roleId, permissionId), ct))
                        .ToNoContentHttpResult(http)
            )
            .WithName("RevokeRolePermission")
            .WithSummary("Revokes a permission from a role.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict
            )
            .Authorize(Permissions.Roles.Manage);

        return app;
    }
}
