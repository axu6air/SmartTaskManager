using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandHandler(IProjectRepository projects, ICurrentUser currentUser)
    : IRequestHandler<CreateProjectCommand, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        var name = request.Name.Trim();

        if (await projects.ExistsAsync(p => p.Name == name, ct))
        {
            return Result<ProjectResponse>.Failure(ProjectErrors.NameTaken);
        }

        // The creator becomes the project's first manager.
        var project = Project.Create(
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            currentUser.GetRequiredUserId()
        );

        await projects.CreateAsync(project, ct);

        return Result<ProjectResponse>.Success(ProjectResponse.From(project));
    }
}
