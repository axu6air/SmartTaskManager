using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;

namespace SmartTaskManager.Application.Features.Projects.Queries.GetProject;

public sealed class GetProjectQueryHandler(ResourceAccess access, ICurrentUser currentUser)
    : IRequestHandler<GetProjectQuery, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(GetProjectQuery request, CancellationToken ct)
    {
        var found = await access.GetProjectAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            Permissions.Projects.View,
            ct
        );

        return found.Map(ProjectResponse.From);
    }
}
