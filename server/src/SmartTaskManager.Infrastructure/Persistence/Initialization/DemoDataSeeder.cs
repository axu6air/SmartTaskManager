using Microsoft.Extensions.Logging;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Infrastructure.Persistence.Initialization;

// Development-only demo data. Every step is skipped when its row already exists, so it is safe to run on each startup.
// There is no signed-in user during startup, so the audit columns are stamped here instead of by an interceptor.
internal sealed class DemoDataSeeder(
    IUserRepository users,
    IRoleRepository roles,
    IProjectRepository projects,
    IProjectTaskRepository tasks,
    IPasswordHasher passwordHasher,
    TimeProvider timeProvider,
    ILogger<DemoDataSeeder> logger)
{
    private const string DemoPassword = "password123";

    public async Task SeedAsync(CancellationToken ct)
    {
        await EnsureUserAsync("Admin", "admin@email.com", RoleNames.Admin, ct);
        var manager1 = await EnsureUserAsync("Manager 1", "manager1@email.com", RoleNames.ProjectManager, ct);
        var manager2 = await EnsureUserAsync("Manager 2", "manager2@email.com", RoleNames.ProjectManager, ct);
        var member11 = await EnsureUserAsync("Member 11", "member11@email.com", RoleNames.TeamMember, ct);
        await EnsureUserAsync("Member 21", "member21@email.com", RoleNames.TeamMember, ct);

        await EnsureProjectAsync("Project 1", "Task 1A", manager1, member11, ct);
        await EnsureProjectAsync("Project 2", "Task 2A", manager2, member11, ct);
    }

    private async Task<User> EnsureUserAsync(string fullName, string email, string roleName, CancellationToken ct)
    {
        var normalizedEmail = User.NormalizeEmail(email);
        var existing = await users.FindAsync(u => u.NormalizedEmail == normalizedEmail, ct: ct);
        if (existing is not null)
        {
            return existing;
        }

        // Tracked, so adding the user does not make EF try to insert the role as well.
        var normalizedRoleName = Role.NormalizeName(roleName);
        var role = await roles.FindAsync(r => r.NormalizedName == normalizedRoleName, tracking: true, ct: ct)
            ?? throw new InvalidOperationException($"The role '{roleName}' is missing. Run database/rbac-schema.sql first.");

        var user = User.Create(fullName, email, passwordHasher.Hash(DemoPassword));
        user.AssignRole(role);

        StampCreated(user, createdBy: null);
        foreach (var userRole in user.UserRoles)
        {
            StampCreated(userRole, createdBy: null);
        }

        await users.CreateAsync(user, ct);
        logger.LogInformation("Created demo user {Email} with role {Role}.", email, roleName);

        return user;
    }

    private async Task EnsureProjectAsync(string name, string taskTitle, User manager, User member, CancellationToken ct)
    {
        if (await projects.ExistsAsync(p => p.Name == name, ct))
        {
            return;
        }

        var project = Project.Create(name, $"Demo {name}.", startDate: null, endDate: null, manager.Id);
        project.AddMember(member.Id);

        StampCreated(project, manager.Id);
        foreach (var projectMember in project.Members)
        {
            StampCreated(projectMember, manager.Id);
        }

        // The project is saved first so the task's composite FK finds the member rows it points to.
        await projects.CreateAsync(project, ct);

        var task = ProjectTask.Create(
            project,
            taskTitle,
            $"Demo {taskTitle}.",
            ProjectTaskPriority.Medium,
            dueDate: null,
            member.Id);

        StampCreated(task, manager.Id);
        await tasks.CreateAsync(task, ct);

        logger.LogInformation("Created demo project {Project} with task {Task}.", name, taskTitle);
    }

    private void StampCreated(ITrackCreated entity, Guid? createdBy)
    {
        entity.CreatedBy = createdBy;
        entity.CreatedAt = timeProvider.GetUtcNow();
    }
}
