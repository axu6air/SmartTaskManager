using MediatR;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects.Commands.RemoveProjectMember;

namespace SmartTaskManager.Api.Endpoints.Projects.Members;

public static class RemoveProjectMemberEndpoint
{
    public static void MapRemoveProjectMember(this IEndpointRouteBuilder app)
    {
        app.MapDelete(
                "/{projectId:guid}/members/{userId:guid}",
                async (Guid projectId, Guid userId, HttpContext http, ISender sender, CancellationToken ct) =>
                {
                    var result = await sender.Send(new RemoveProjectMemberCommand(projectId, userId), ct);
                    return result.ToNoContentHttpResult(http);
                }
            )
            .WithName("RemoveProjectMember")
            .WithSummary("Removes a member from the project and unassigns their tasks in it.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesErrors(
                StatusCodes.Status400BadRequest,
                StatusCodes.Status401Unauthorized,
                StatusCodes.Status403Forbidden,
                StatusCodes.Status404NotFound
            )
            .Authorize(Permissions.Projects.ManageMembers);
    }
}
