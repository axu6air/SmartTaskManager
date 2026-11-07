using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Projects;

public sealed record ProjectMemberResponse(Guid UserId, bool IsManager);

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate,
    IReadOnlyCollection<ProjectMemberResponse> Members
)
{
    // The project's Members must be loaded.
    public static ProjectResponse From(Project project) =>
        new(
            project.Id,
            project.Name,
            project.Description,
            project.StartDate,
            project.EndDate,
            project.Members.Select(m => new ProjectMemberResponse(m.UserId, m.IsManager)).ToList()
        );
}

// A list row: no members, so listing never has to load them.
public sealed record ProjectSummary(
    Guid Id,
    string Name,
    string? Description,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate
)
{
    public static ProjectSummary From(Project project) =>
        new(project.Id, project.Name, project.Description, project.StartDate, project.EndDate);
}
