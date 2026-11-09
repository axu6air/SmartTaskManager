using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Common.Security;
using SmartTaskManager.Application.Features.Projects.Commands.RemoveProjectMember;
using SmartTaskManager.Domain.Exceptions;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.UnitTests.Features.Projects;

public class RemoveProjectMemberCommandHandlerTests
{
    private readonly Guid _managerId = Guid.NewGuid();
    private readonly Guid _memberId = Guid.NewGuid();
    private readonly IProjectRepository _projects = Substitute.For<IProjectRepository>();
    private readonly IProjectTaskRepository _tasks = Substitute.For<IProjectTaskRepository>();
    private readonly IPermissionService _permissions = Substitute.For<IPermissionService>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly Project _project;

    public RemoveProjectMemberCommandHandlerTests()
    {
        _project = Project.Create("Project", null, null, null, _managerId);
        _project.AddMember(_memberId);
        _currentUser.GetRequiredUserId().Returns(_managerId);
        _projects.FindWithMembersAsync(default, default).ReturnsForAnyArgs(_project);
        _permissions.GetScopesAsync(default, default!, default).ReturnsForAnyArgs(PermissionScopes.Managed);
        _tasks.FindAssignedIncludingDeletedAsync(default, default, default).ReturnsForAnyArgs([]);
    }

    private RemoveProjectMemberCommandHandler Handler =>
        new(new ResourceAccess(_projects, _tasks, _permissions), _projects, _tasks, _currentUser);

    [Fact]
    public async Task Handle_Member_RemovesThemAndUnassignsTheirTasks()
    {
        var task = ProjectTask.Create(_project, "Task", null, ProjectTaskPriority.Medium, null, _memberId);
        _tasks.FindAssignedIncludingDeletedAsync(default, default, default).ReturnsForAnyArgs([task]);

        var result = await Handler.Handle(new RemoveProjectMemberCommand(_project.Id, _memberId), default);

        result.IsSuccess.Should().BeTrue();
        _project.IsMember(_memberId).Should().BeFalse();
        task.AssigneeId.Should().BeNull();
        await _projects.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotAMember_ReturnsNotFound()
    {
        var result = await Handler.Handle(new RemoveProjectMemberCommand(_project.Id, Guid.NewGuid()), default);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_LastManager_IsRefusedByTheDomain()
    {
        var act = () => Handler.Handle(new RemoveProjectMemberCommand(_project.Id, _managerId), default);

        // DomainExceptionBehavior turns this into a 400 in the pipeline.
        await act.Should().ThrowAsync<DomainException>();
        await _projects.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_ProjectNotManagedByUser_ReturnsForbidden()
    {
        _currentUser.GetRequiredUserId().Returns(Guid.NewGuid());

        var result = await Handler.Handle(new RemoveProjectMemberCommand(_project.Id, _memberId), default);

        result.Error!.Type.Should().Be(ErrorType.Forbidden);
    }
}
