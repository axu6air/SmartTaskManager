using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class PermissionModuleRepository(ApplicationDbContext context)
    : Repository<PermissionModule, ApplicationDbContext>(context),
        IPermissionModuleRepository;
