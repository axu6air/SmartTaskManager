using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.UnitTests.Common;

public class ResourceAccessTests
{
    private static readonly Guid Manager = Guid.NewGuid();
    private static readonly Guid Outsider = Guid.NewGuid();

    private readonly IProjectRepository _projects = Substitute.For<IProjectRepository>();
    private readonly IProjectTaskRepository _tasks = Substitute.For<IProjectTaskRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();
    private readonly Project _project = Project.Create("Project", null, null, null, Manager);

    private ResourceAccess CreateAccess(PermissionScopes? scopes, Project? found)
    {
        _projects.FindWithMembersAsync(default, default).ReturnsForAnyArgs(found);
        _permissions.GetScopesAsync(default, default!, default).ReturnsForAnyArgs(scopes);

        return new ResourceAccess(_projects, _tasks, _permissions);
    }

    [Fact]
    public async Task GetProjectAsync_UnknownProject_ReturnsNotFound()
    {
        var result = await CreateAccess(PermissionScopes.All, null).GetProjectAsync(Manager, Guid.NewGuid(), "p", default);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetProjectAsync_ProjectOutsideTheScope_ReturnsForbidden()
    {
        var result = await CreateAccess(PermissionScopes.Member, _project).GetProjectAsync(Outsider, Guid.NewGuid(), "p", default);

        result.Error.Should().Be(Errors.Forbidden);
    }

    [Fact]
    public async Task GetProjectAsync_PermissionNotHeld_ReturnsForbidden()
    {
        var result = await CreateAccess(null, _project).GetProjectAsync(Manager, Guid.NewGuid(), "p", default);

        result.Error.Should().Be(Errors.Forbidden);
    }

    [Fact]
    public async Task GetProjectAsync_ProjectInsideTheScope_ReturnsTheProject()
    {
        var result = await CreateAccess(PermissionScopes.Managed, _project).GetProjectAsync(Manager, Guid.NewGuid(), "p", default);

        result.Value.Should().BeSameAs(_project);
    }

    [Fact]
    public async Task GetTaskAsync_TaskOfAnotherProject_ReturnsNotFound()
    {
        var task = ProjectTask.Create(_project, "Task", null, ProjectTaskPriority.Low, null, null);
        _tasks.FindAsync(default, default, default).ReturnsForAnyArgs(task);

        var result = await CreateAccess(PermissionScopes.All, _project).GetTaskAsync(
            Manager, Guid.NewGuid(), Guid.NewGuid(), "t", default);

        // _project.Id and the random route project ID differ only when the task was reached through the wrong URL.
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }
}
