using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects;
using SmartTaskManager.Application.Features.Projects.Queries.GetProject;

namespace SmartTaskManager.Api.Endpoints.Projects.List;

public static class GetProjectEndpoint
{
    public static void MapGetProject(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/{projectId:guid}",
                async (Guid projectId, HttpContext http, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new GetProjectQuery(projectId), ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("GetProject")
            .WithSummary("Gets a project with its members.")
            .Produces<ApiResponse<ProjectResponse>>()
            .ProducesErrors(
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Projects.View);
    }
}
