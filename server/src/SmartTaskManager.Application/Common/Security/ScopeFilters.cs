using System.Linq.Expressions;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Common.Security;

// The one definition of what each scope means. List queries use the expressions as a Where filter;
// single-resource actions evaluate the same expressions in memory through CanAccess.
// Scopes are flags, not a hierarchy: access is granted when any set flag's condition holds.
public static class ScopeFilters
{
    // The Assigned scope has no meaning for a project, so it never grants access here.
    public static Expression<Func<Project, bool>> Projects(Guid userId, PermissionScopes scopes)
    {
        var all = scopes.HasFlag(PermissionScopes.All);
        var managed = scopes.HasFlag(PermissionScopes.Managed);
        var member = scopes.HasFlag(PermissionScopes.Member);

        return p =>
            all
            || (managed && p.Members.Any(m => m.UserId == userId && m.IsManager))
            || (member && p.Members.Any(m => m.UserId == userId));
    }

    // Always limited to one project, which also stops a task being reached through another project's URL.
    public static Expression<Func<ProjectTask, bool>> Tasks(Guid userId, PermissionScopes scopes, Guid projectId)
    {
        var all = scopes.HasFlag(PermissionScopes.All);
        var assigned = scopes.HasFlag(PermissionScopes.Assigned);
        var managed = scopes.HasFlag(PermissionScopes.Managed);
        var member = scopes.HasFlag(PermissionScopes.Member);

        return t =>
            t.ProjectId == projectId
            && (all
                || (assigned && t.AssigneeId == userId)
                || (managed && t.Project.Members.Any(m => m.UserId == userId && m.IsManager))
                || (member && t.Project.Members.Any(m => m.UserId == userId)));
    }

    // The project's Members must be loaded.
    public static bool CanAccess(Project project, Guid userId, PermissionScopes scopes) =>
        Projects(userId, scopes).Compile()(project);

    // The task's Project, with its Members, must be loaded.
    public static bool CanAccess(ProjectTask task, Guid userId, PermissionScopes scopes) =>
        Tasks(userId, scopes, task.ProjectId).Compile()(task);
}
