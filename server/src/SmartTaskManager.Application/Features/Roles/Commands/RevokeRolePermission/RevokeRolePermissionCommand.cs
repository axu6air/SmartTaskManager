using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Roles.Commands.RevokeRolePermission;

public sealed record RevokeRolePermissionCommand(Guid RoleId, Guid PermissionId) : IRequest<Result<bool>>;
