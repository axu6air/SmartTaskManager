using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects.Queries.GetProjectMembers;

namespace SmartTaskManager.Api.Endpoints.Projects.Members;

public static class GetProjectMembersEndpoint
{
    public static void MapGetProjectMembers(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/{projectId:guid}/members",
                async (Guid projectId, HttpContext http, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetProjectMembersQuery(projectId), ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("GetProjectMembers")
            .WithSummary("Lists a project's members with their names.")
            .Produces<ApiResponse<IReadOnlyCollection<ProjectMemberDetails>>>()
            .ProducesErrors(
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Projects.View);
    }
}
