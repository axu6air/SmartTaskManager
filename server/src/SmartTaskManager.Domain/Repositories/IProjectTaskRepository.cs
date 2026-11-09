using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Domain.Repositories;

public interface IProjectTaskRepository : IRepository<ProjectTask>
{
    /// <summary>
    /// Loads, tracked, every task in the project assigned to the user, soft-deleted ones included:
    /// the (ProjectId, AssigneeId) foreign key to ProjectMembers applies to deleted rows too.
    /// </summary>
    Task<ICollection<ProjectTask>> FindAssignedIncludingDeletedAsync(
        Guid projectId,
        Guid assigneeId,
        CancellationToken ct = default
    );
}
