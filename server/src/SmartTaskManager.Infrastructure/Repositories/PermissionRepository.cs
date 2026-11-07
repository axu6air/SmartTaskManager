using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class PermissionRepository(ApplicationDbContext context)
    : Repository<Permission, ApplicationDbContext>(context),
        IPermissionRepository;
