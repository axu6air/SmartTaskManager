using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Roles.Queries.GetRoles;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Application.Features.Roles.Commands.GrantRolePermission;

// Grants the permission, or changes its scope when the role already holds it.
public sealed record GrantRolePermissionCommand(Guid RoleId, Guid PermissionId, PermissionScopes Scope)
    : IRequest<Result<RoleGrantResponse>>;
