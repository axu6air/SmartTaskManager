using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects;
using SmartTaskManager.Application.Features.Projects.Commands.UpdateProject;

namespace SmartTaskManager.Api.Endpoints.Projects.Update;

public static class UpdateProjectEndpoint
{
    public static void MapUpdateProject(this IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/{projectId:guid}",
                async (
                    Guid projectId,
                    UpdateProjectRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new UpdateProjectCommand(
                        projectId,
                        request.Name,
                        request.Description,
                        request.StartDate,
                        request.EndDate
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("UpdateProject")
            .WithSummary("Updates a project's name, description and dates.")
            .Produces<ApiResponse<ProjectResponse>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound,
                StatusCodes.Status409Conflict
            )
            .AddEndpointFilter<ValidationFilter<UpdateProjectRequest>>()
            .Authorize(Permissions.Projects.Update);
    }
}
