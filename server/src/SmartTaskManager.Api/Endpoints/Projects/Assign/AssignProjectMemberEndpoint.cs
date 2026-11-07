using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects;
using SmartTaskManager.Application.Features.Projects.Commands.AssignProjectMember;

namespace SmartTaskManager.Api.Endpoints.Projects.Assign;

public static class AssignProjectMemberEndpoint
{
    public static void MapAssignProjectMember(this IEndpointRouteBuilder app)
    {
        app.MapPatch(
                "/{projectId:guid}/assign",
                async (
                    Guid projectId,
                    AssignProjectMemberRequest request,
                    HttpContext http,
                    ISender sender,
                    CancellationToken ct
                ) =>
                {
                    var command = new AssignProjectMemberCommand(
                        projectId,
                        request.UserId,
                        request.IsManager
                    );

                    var result = await sender.Send(command, ct);
                    return result.ToHttpResult(http);
                }
            )
            .WithName("AssignProjectMember")
            .WithSummary(
                "Adds a user to the project, or changes their manager flag if they are already a member."
            )
            .Produces<ApiResponse<ProjectResponse>>()
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Projects.ManageMembers);
    }
}
