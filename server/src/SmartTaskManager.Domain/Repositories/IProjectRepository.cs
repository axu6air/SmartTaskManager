using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Domain.Repositories;

public interface IProjectRepository : IRepository<Project>
{
    /// <summary>
    /// Loads a tracked project with <see cref="Project.Members"/>, ready for the member methods
    /// and for assignee checks in <see cref="ProjectTask"/>.
    /// </summary>
    Task<Project?> FindWithMembersAsync(Guid id, CancellationToken ct = default);
}
