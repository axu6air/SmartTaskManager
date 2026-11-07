using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;
using SmartTaskManager.Infrastructure.Persistence;
using SmartTaskManager.Infrastructure.Repositories.Abstractions;

namespace SmartTaskManager.Infrastructure.Repositories;

public class ProjectTaskRepository(ApplicationDbContext context)
    : Repository<ProjectTask, ApplicationDbContext>(context),
        IProjectTaskRepository;
