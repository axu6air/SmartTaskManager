namespace SmartTaskManager.Domain.Models.Projects;

public class ProjectMember : ITrackCreated, ITrackUpdated
{
    private ProjectMember() { }

    internal ProjectMember(Project project, Guid userId, bool isManager)
    {
        Project = project;
        ProjectId = project.Id;
        UserId = userId;
        IsManager = isManager;
    }

    public Guid ProjectId { get; private set; }

    public Project Project { get; private set; } = null!;

    public Guid UserId { get; private set; }

    public bool IsManager { get; internal set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
