using FluentAssertions;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.UnitTests.Common;

public class ScopeFiltersTests
{
    private static readonly Guid Manager = Guid.NewGuid();
    private static readonly Guid Member = Guid.NewGuid();
    private static readonly Guid Outsider = Guid.NewGuid();

    private readonly Project _project;
    private readonly ProjectTask _task;

    public ScopeFiltersTests()
    {
        _project = Project.Create("Project", null, null, null, Manager);
        _project.AddMember(Member);
        _task = ProjectTask.Create(_project, "Task", null, ProjectTaskPriority.Medium, null, Member);
    }

    [Theory]
    [InlineData(PermissionScopes.All, true, true, true)]
    [InlineData(PermissionScopes.Managed, true, false, false)]
    [InlineData(PermissionScopes.Member, true, true, false)]
    [InlineData(PermissionScopes.Assigned, false, false, false)]
    [InlineData(PermissionScopes.Managed | PermissionScopes.Member, true, true, false)]
    public void CanAccess_Project_FollowsTheScopeFlags(
        PermissionScopes scopes, bool manager, bool member, bool outsider)
    {
        ScopeFilters.CanAccess(_project, Manager, scopes).Should().Be(manager);
        ScopeFilters.CanAccess(_project, Member, scopes).Should().Be(member);
        ScopeFilters.CanAccess(_project, Outsider, scopes).Should().Be(outsider);
    }

    [Theory]
    [InlineData(PermissionScopes.All, true, true, true)]
    [InlineData(PermissionScopes.Assigned, false, true, false)]
    [InlineData(PermissionScopes.Managed, true, false, false)]
    [InlineData(PermissionScopes.Member, true, true, false)]
    public void CanAccess_Task_FollowsTheScopeFlags(
        PermissionScopes scopes, bool manager, bool assignee, bool outsider)
    {
        ScopeFilters.CanAccess(_task, Manager, scopes).Should().Be(manager);
        ScopeFilters.CanAccess(_task, Member, scopes).Should().Be(assignee);
        ScopeFilters.CanAccess(_task, Outsider, scopes).Should().Be(outsider);
    }

    [Fact]
    public void CanAccess_NoScopes_DeniesEveryone()
    {
        ScopeFilters.CanAccess(_project, Manager, default).Should().BeFalse();
        ScopeFilters.CanAccess(_task, Manager, default).Should().BeFalse();
    }

    [Fact]
    public void Tasks_FilterForAnotherProject_DoesNotMatch()
    {
        var filter = ScopeFilters.Tasks(Manager, PermissionScopes.All, Guid.NewGuid()).Compile();

        filter(_task).Should().BeFalse();
    }
}
