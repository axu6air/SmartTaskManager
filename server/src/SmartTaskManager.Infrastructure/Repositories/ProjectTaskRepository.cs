using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class ProjectTaskRepository(ApplicationDbContext context)
    : Repository<ProjectTask, ApplicationDbContext>(context),
        IProjectTaskRepository
{
    public Task<ICollection<ProjectTask>> FindAssignedIncludingDeletedAsync(
        Guid projectId,
        Guid assigneeId,
        CancellationToken ct = default
    )
    {
        return ExecuteAsync<ICollection<ProjectTask>>(
            async (ct) =>
                await DbSet
                    .AsTracking()
                    // Deliberate: soft-deleted tasks still hold the assignee FK, so they must be unassigned too.
                    .IgnoreQueryFilters()
                    .Where(t => t.ProjectId == projectId && t.AssigneeId == assigneeId)
                    .ToListAsync(ct),
            ct
        );
    }
}
