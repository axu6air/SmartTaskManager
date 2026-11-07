using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Api.Endpoints.Projects.Create;
using SmartTaskManager.Api.Endpoints.Projects.Update;
using SmartTaskManager.Api.Endpoints.Tasks.Create;
using SmartTaskManager.Api.Endpoints.Tasks.Update;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Api.IntegrationTests.Endpoints;

public class RequestValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    private static CreateTaskRequestValidator CreateTaskValidator()
    {
        var time = Substitute.For<TimeProvider>();
        time.GetUtcNow().Returns(Now);

        return new CreateTaskRequestValidator(time);
    }

    [Fact]
    public void CreateProject_ValidRequest_IsValid()
    {
        var request = new CreateProjectRequest("Apollo", "d", Now, Now.AddDays(1));

        new CreateProjectRequestValidator().Validate(request).IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateProject_BlankName_IsInvalid(string name)
    {
        new CreateProjectRequestValidator().Validate(new CreateProjectRequest(name, null, null, null))
            .IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateProject_EndBeforeStart_IsInvalid()
    {
        var request = new CreateProjectRequest("Apollo", null, Now, Now.AddDays(-1));

        new CreateProjectRequestValidator().Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateProject_NameLongerThanTheColumn_IsInvalid()
    {
        var request = new UpdateProjectRequest(new string('a', 201), null, null, null);

        new UpdateProjectRequestValidator().Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateProject_ValidRequest_IsValid()
    {
        new UpdateProjectRequestValidator().Validate(new UpdateProjectRequest("Apollo", null, null, null))
            .IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateTask_ValidRequest_IsValid()
    {
        var request = new CreateTaskRequest("Title", null, ProjectTaskPriority.High, Now.AddDays(1), null);

        CreateTaskValidator().Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateTask_DueDateInThePast_IsInvalid()
    {
        var request = new CreateTaskRequest("Title", null, ProjectTaskPriority.High, Now.AddDays(-1), null);

        CreateTaskValidator().Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateTask_UndefinedPriority_IsInvalid()
    {
        var request = new CreateTaskRequest("Title", null, (ProjectTaskPriority)9, null, null);

        CreateTaskValidator().Validate(request).IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateTask_PastDueDate_IsStillValid()
    {
        var request = new UpdateTaskRequest("Title", null, ProjectTaskPriority.Low, Now.AddDays(-30));

        new UpdateTaskRequestValidator().Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateTask_BlankTitle_IsInvalid()
    {
        new UpdateTaskRequestValidator().Validate(new UpdateTaskRequest("", null, ProjectTaskPriority.Low, null))
            .IsValid.Should().BeFalse();
    }
}
