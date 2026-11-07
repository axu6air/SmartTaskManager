using SmartTaskManager.Domain.Models.Identity;

namespace SmartTaskManager.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Loads a tracked user with <see cref="User.UserRoles"/>, ready for <see cref="User.AssignRole"/>.
    /// </summary>
    Task<User?> FindWithRolesAsync(Guid id, CancellationToken ct = default);
}
