using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects.Commands.RemoveProjectMember;

public sealed record RemoveProjectMemberCommand(Guid ProjectId, Guid UserId) : IRequest<Result<bool>>;
