using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Roles.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<Result<RolesResponse>>;
