using FluentAssertions;
using NSubstitute;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Features.Projects.Commands.CreateProject;
using SmartTaskManager.Application.Features.Tasks.Commands.CreateTask;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.UnitTests.Features;

public class ValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

    private static CreateTaskCommandValidator CreateTaskValidator()
    {
        var time = Substitute.For<TimeProvider>();
        time.GetUtcNow().Returns(Now);

        return new CreateTaskCommandValidator(time);
    }

    private static CreateTaskCommand Task(DateTimeOffset? dueDate, ProjectTaskPriority priority = ProjectTaskPriority.Low) =>
        new(Guid.NewGuid(), "Title", null, priority, dueDate, null);

    [Fact]
    public void CreateTask_DueDateInThePast_IsInvalid()
    {
        CreateTaskValidator().Validate(Task(Now.AddDays(-1))).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    public void CreateTask_DueDateNowOrLater_IsValid(int days)
    {
        CreateTaskValidator().Validate(Task(Now.AddDays(days))).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateTask_NoDueDate_IsValid()
    {
        CreateTaskValidator().Validate(Task(null)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateTask_UndefinedPriority_IsInvalid()
    {
        CreateTaskValidator().Validate(Task(null, (ProjectTaskPriority)9)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateProject_EndBeforeStart_IsInvalid()
    {
        var command = new CreateProjectCommand("Apollo", null, Now, Now.AddDays(-1));

        new CreateProjectCommandValidator().Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateProject_BlankName_IsInvalid()
    {
        new CreateProjectCommandValidator().Validate(new CreateProjectCommand(" ", null, null, null))
            .IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(3, 1)]
    public void Normalize_PageAndPageSize_AreClampedToTheirLimits(int page, int pageSize)
    {
        var (normalizedPage, normalizedSize) = Paging.Normalize(page, pageSize);

        normalizedPage.Should().BeGreaterThanOrEqualTo(1);
        normalizedSize.Should().Be(1);
    }

    [Fact]
    public void Normalize_HugePageSize_IsCappedAtTheMaximum()
    {
        Paging.Normalize(1, 100_000).PageSize.Should().Be(Paging.MaxPageSize);
    }
}
