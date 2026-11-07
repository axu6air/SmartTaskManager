using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Common.Security;

// Loads a project or task for a single-resource action and checks the user's scope for the permission.
// The loaded entities are tracked, so handlers can change them and call SaveChangesAsync.
public sealed class ResourceAccess(
    IProjectRepository projects,
    IProjectTaskRepository tasks,
    IPermissionService permissions
)
{
    public async Task<Result<Project>> GetProjectAsync(
        Guid userId,
        Guid projectId,
        string permissionCode,
        CancellationToken ct
    )
    {
        var project = await projects.FindWithMembersAsync(projectId, ct);

        if (project is null)
        {
            return Result<Project>.Failure(Errors.NotFound("Project"));
        }

        var scopes = await permissions.GetScopesAsync(userId, permissionCode, ct) ?? default;

        return ScopeFilters.CanAccess(project, userId, scopes)
            ? Result<Project>.Success(project)
            : Result<Project>.Failure(Errors.Forbidden);
    }

    // The returned task's Project is loaded with its members.
    public async Task<Result<ProjectTask>> GetTaskAsync(
        Guid userId,
        Guid projectId,
        Guid taskId,
        string permissionCode,
        CancellationToken ct
    )
    {
        // The project is loaded first so the task's Project is this tracked instance, members included.
        var project = await projects.FindWithMembersAsync(projectId, ct);

        if (project is null)
        {
            return Result<ProjectTask>.Failure(Errors.NotFound("Project"));
        }

        var task = await tasks.FindAsync(taskId, tracking: true, ct: ct);

        if (task is null || task.ProjectId != projectId)
        {
            return Result<ProjectTask>.Failure(Errors.NotFound("Task"));
        }

        var scopes = await permissions.GetScopesAsync(userId, permissionCode, ct) ?? default;

        return ScopeFilters.CanAccess(task, userId, scopes)
            ? Result<ProjectTask>.Success(task)
            : Result<ProjectTask>.Failure(Errors.Forbidden);
    }
}
