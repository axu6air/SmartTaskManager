using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Domain.Models.Projects;

public class ProjectTask : Entity, ITrackCreated, ITrackUpdated, ISoftDeletable
{
    private ProjectTask() { }

    public Guid ProjectId { get; private set; }

    public Project Project { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public ProjectTaskStatus Status { get; private set; }

    public ProjectTaskPriority Priority { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public Guid? AssigneeId { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public Guid? DeletedBy { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public byte[] RowVersion { get; private set; } = [];

    // The project must have its members loaded, because the assignee is checked against them.
    public static ProjectTask Create(
        Project project,
        string title,
        string? description,
        ProjectTaskPriority priority,
        DateTimeOffset? dueDate,
        Guid? assigneeId
    )
    {
        ArgumentNullException.ThrowIfNull(project);

        var task = new ProjectTask
        {
            Project = project,
            ProjectId = project.Id,
            Status = ProjectTaskStatus.ToDo,
        };

        task.Update(title, description, priority, dueDate);
        task.Assign(project, assigneeId);

        return task;
    }

    public void Update(
        string title,
        string? description,
        ProjectTaskPriority priority,
        DateTimeOffset? dueDate
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        if (!Enum.IsDefined(priority))
        {
            throw new DomainException("The task priority is not valid.");
        }

        Title = title.Trim();
        Description = description;
        Priority = priority;
        DueDate = dueDate;
    }

    public void Assign(Project project, Guid? assigneeId)
    {
        ArgumentNullException.ThrowIfNull(project);

        if (project.Id != ProjectId)
        {
            throw new DomainException("The task does not belong to this project.");
        }

        if (assigneeId.HasValue && !project.IsMember(assigneeId.Value))
        {
            throw new DomainException("The assignee must be a member of the task's project.");
        }

        AssigneeId = assigneeId;
    }

    public void ChangeStatus(ProjectTaskStatus status, DateTimeOffset now)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException("The task status is not valid.");
        }

        if (status == Status)
        {
            return;
        }

        Status = status;
        CompletedAt = status == ProjectTaskStatus.Completed ? now : null;
    }

    public void MarkDeleted(Guid? deletedBy, DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = deletedAt;
    }
}
