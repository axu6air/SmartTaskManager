using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects;
using SmartTaskManager.Application.Features.Projects.Commands.CreateProject;

namespace SmartTaskManager.Api.Endpoints.Projects.Create;

public static class CreateProjectEndpoint
{
    public static void MapCreateProject(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/",
                async (
                    CreateProjectRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new CreateProjectCommand(
                        request.Name,
                        request.Description,
                        request.StartDate,
                        request.EndDate
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http, project => $"/api/projects/{project.Id}");
                }
            )
            .WithName("CreateProject")
            .WithSummary("Creates a project. The creator becomes its first manager.")
            .Produces<ApiResponse<ProjectResponse>>(StatusCodes.Status201Created)
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status409Conflict
            )
            .AddEndpointFilter<ValidationFilter<CreateProjectRequest>>()
            .Authorize(Permissions.Projects.Create);
    }
}
