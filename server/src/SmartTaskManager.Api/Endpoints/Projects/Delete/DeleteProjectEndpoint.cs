using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects.Commands.DeleteProject;

namespace SmartTaskManager.Api.Endpoints.Projects.Delete;

public static class DeleteProjectEndpoint
{
    public static void MapDeleteProject(this IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/{projectId:guid}",
                async (Guid projectId, HttpContext http, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new DeleteProjectCommand(projectId), ct);
                    return result.ToNoContentHttpResult(http);
                }
            )
            .WithName("DeleteProject")
            .WithSummary("Deletes a project (soft delete).")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Projects.Delete);
    }
}
