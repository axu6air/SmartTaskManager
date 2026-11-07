using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Application.Features.Tasks.Commands.CreateTask;

namespace SmartTaskManager.Api.Endpoints.Tasks.Create;

public static class CreateTaskEndpoint
{
    public static void MapCreateTask(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/",
                async (
                    Guid projectId,
                    CreateTaskRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new CreateTaskCommand(
                        projectId,
                        request.Title,
                        request.Description,
                        request.Priority,
                        request.DueDate,
                        request.AssigneeId
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(
                        http,
                        task => $"/api/projects/{projectId}/tasks/{task.Id}"
                    );
                }
            )
            .WithName("CreateTask")
            .WithSummary(
                "Creates a task in a project. The assignee, if given, must be a member of the project."
            )
            .Produces<ApiResponse<ProjectTaskResponse>>(StatusCodes.Status201Created)
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .AddEndpointFilter<ValidationFilter<CreateTaskRequest>>()
            .Authorize(Permissions.Tasks.Create);
    }
}
