using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class ProjectRepository(ApplicationDbContext context)
    : Repository<Project, ApplicationDbContext>(context),
        IProjectRepository
{
    public Task<Project?> FindWithMembersAsync(Guid id, CancellationToken ct = default)
    {
        return ExecuteAsync(
            (ct) =>
                DbSet.AsTracking().Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == id, ct),
            ct
        );
    }
}
