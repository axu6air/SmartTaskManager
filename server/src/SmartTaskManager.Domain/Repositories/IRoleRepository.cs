using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Loads a tracked role with <see cref="Role.RolePermissions"/>, ready for <see cref="Role.Grant"/>.
    /// </summary>
    Task<Role?> FindWithPermissionsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Loads every role, untracked and ordered by name, with <see cref="Role.RolePermissions"/>.
    /// </summary>
    Task<ICollection<Role>> FindAllWithPermissionsAsync(CancellationToken ct = default);
}
