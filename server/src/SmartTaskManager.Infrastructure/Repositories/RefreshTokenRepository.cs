using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class RefreshTokenRepository(ApplicationDbContext context)
    : Repository<RefreshToken, ApplicationDbContext>(context),
        IRefreshTokenRepository;
