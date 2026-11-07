using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Domain.Models.Projects;

public class Project : Entity, ITrackCreated, ITrackUpdated, ISoftDeletable
{
    private readonly List<ProjectMember> _members = [];

    private Project() { }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTimeOffset? StartDate { get; private set; }

    public DateTimeOffset? EndDate { get; private set; }

    public bool IsDeleted { get; private set; }

    public Guid? DeletedBy { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public IReadOnlyCollection<ProjectMember> Members => _members;

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public byte[] RowVersion { get; private set; } = [];

    // The creator becomes the first manager, so a new project always has at least one manager.
    public static Project Create(
        string name,
        string? description,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate,
        Guid creatorId
    )
    {
        var project = new Project();
        project.Update(name, description, startDate, endDate);
        project._members.Add(new ProjectMember(project, creatorId, isManager: true));

        return project;
    }

    public void Update(
        string name,
        string? description,
        DateTimeOffset? startDate,
        DateTimeOffset? endDate
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            throw new DomainException("The end date cannot be before the start date.");
        }

        Name = name.Trim();
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
    }

    // Members must be loaded before calling any of the member methods below.
    public bool IsMember(Guid userId) => _members.Any(m => m.UserId == userId);

    public bool IsManager(Guid userId) => _members.Any(m => m.UserId == userId && m.IsManager);

    public void AddMember(Guid userId, bool isManager = false)
    {
        if (IsMember(userId))
        {
            throw new DomainException("The user is already a member of this project.");
        }

        _members.Add(new ProjectMember(this, userId, isManager));
    }

    public void SetManager(Guid userId, bool isManager)
    {
        var member = FindMember(userId);

        if (!isManager && member.IsManager)
        {
            EnsureAnotherManagerExists(userId);
        }

        member.IsManager = isManager;
    }

    public void RemoveMember(Guid userId)
    {
        var member = FindMember(userId);

        if (member.IsManager)
        {
            EnsureAnotherManagerExists(userId);
        }

        _members.Remove(member);
    }

    public void MarkDeleted(Guid? deletedBy, DateTimeOffset deletedAt)
    {
        IsDeleted = true;
        DeletedBy = deletedBy;
        DeletedAt = deletedAt;
    }

    private ProjectMember FindMember(Guid userId)
    {
        return _members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new DomainException("The user is not a member of this project.");
    }

    private void EnsureAnotherManagerExists(Guid excludedUserId)
    {
        if (!_members.Any(m => m.IsManager && m.UserId != excludedUserId))
        {
            throw new DomainException("A project must keep at least one manager.");
        }
    }
}
