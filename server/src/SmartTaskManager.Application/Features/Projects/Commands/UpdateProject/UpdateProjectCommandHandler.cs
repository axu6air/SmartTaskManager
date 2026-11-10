using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Projects.Commands.UpdateProject;

public sealed class UpdateProjectCommandHandler(
    ResourceAccess access,
    IProjectRepository projects,
    ICurrentUser currentUser
) : IRequestHandler<UpdateProjectCommand, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(UpdateProjectCommand request, CancellationToken ct)
    {
        var found = await access.GetProjectAsync(
            currentUser.GetRequiredUserId(),
            request.ProjectId,
            Permissions.Projects.Update,
            ct
        );

        if (!found.IsSuccess)
        {
            return Result<ProjectResponse>.Failure(found.Error!);
        }

        var project = found.Value!;
        var name = request.Name.Trim();

        if (await projects.ExistsAsync(p => p.Name == name && p.Id != project.Id, ct))
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.NameTaken);
        }

        project.Update(
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.IsAiModified
        );
        await projects.SaveChangesAsync(ct);

        return Result<ProjectResponse>.Success(ProjectResponse.From(project));
    }
}
