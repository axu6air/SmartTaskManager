using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;

namespace SmartTaskManager.Application.Features.Tasks.Queries.GetTask;

public sealed class GetTaskQueryHandler(ResourceAccess access, ICurrentUser currentUser)
    : IRequestHandler<GetTaskQuery, Result<ProjectTaskResponse>>
{
    public async Task<Result<ProjectTaskResponse>> Handle(GetTaskQuery request, CancellationToken ct)
    {
        var found = await access.GetTaskAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            request.TaskId,
            Permissions.Tasks.View,
            ct
        );

        return found.Map(ProjectTaskResponse.From);
    }
}
