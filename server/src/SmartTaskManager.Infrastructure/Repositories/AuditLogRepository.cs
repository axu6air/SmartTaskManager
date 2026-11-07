using SmartTaskManager.Domain.Models.Auditing;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class AuditLogRepository(ApplicationDbContext context)
    : Repository<AuditLog, ApplicationDbContext>(context),
        IAuditLogRepository;
