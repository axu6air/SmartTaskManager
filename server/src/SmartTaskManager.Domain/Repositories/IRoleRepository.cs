using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Domain.Repositories;

public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Loads a tracked role with <see cref="Role.RolePermissions"/>, ready for <see cref="Role.Grant"/>.
    /// </summary>
    Task<Role?> FindWithPermissionsAsync(Guid id, CancellationToken ct = default);
}
