using System.Linq.Expressions;
using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Application.Features.Projects;
using SmartTaskManager.Application.Features.Projects.Commands.CreateProject;
using SmartTaskManager.Domain.Models.Projects;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.UnitTests.Features.Projects;

public class CreateProjectCommandHandlerTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly IProjectRepository _projects = Substitute.For<IProjectRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private CreateProjectCommandHandler CreateHandler(bool nameTaken)
    {
        _currentUser.GetRequiredUserId().Returns(UserId);
        _projects.ExistsAsync(Arg.Any<Expression<Func<Project, bool>>>(), Arg.Any<CancellationToken>()).Returns(nameTaken);

        return new CreateProjectCommandHandler(_projects, _currentUser);
    }

    [Fact]
    public async Task Handle_NewName_CreatesTheProjectWithTheCreatorAsManager()
    {
        var result = await CreateHandler(nameTaken: false).Handle(new CreateProjectCommand("Apollo", null, null, null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Members.Should().ContainSingle(m => m.UserId == UserId && m.IsManager);
        await _projects.Received(1).CreateAsync(Arg.Is<Project>(p => p.Name == "Apollo"), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NameAlreadyInUse_ReturnsConflict()
    {
        var result = await CreateHandler(nameTaken: true).Handle(new CreateProjectCommand("Apollo", null, null, null), default);

        result.Error.Should().Be(ProjectErrors.NameTaken);
        await _projects.DidNotReceiveWithAnyArgs().CreateAsync(default!, default);
    }
}
