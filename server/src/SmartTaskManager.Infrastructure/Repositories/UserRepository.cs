using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class UserRepository(ApplicationDbContext context)
    : Repository<User, ApplicationDbContext>(context),
        IUserRepository
{
    public Task<User?> FindWithRolesAsync(Guid id, CancellationToken ct = default)
    {
        return ExecuteAsync(
            (ct) =>
                DbSet
                    .AsTracking()
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Id == id, ct),
            ct
        );
    }
}
