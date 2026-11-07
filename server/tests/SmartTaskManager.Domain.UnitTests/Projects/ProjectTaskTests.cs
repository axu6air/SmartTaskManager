using FluentAssertions;
using SmartTaskManager.Domain.Exceptions;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Domain.UnitTests.Projects;

public class ProjectTaskTests
{
    private static readonly Guid Member = Guid.NewGuid();
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static Project NewProject()
    {
        var project = Project.Create("Apollo", null, null, null, Guid.NewGuid());
        project.AddMember(Member);
        return project;
    }

    private static ProjectTask NewTask(Project project, Guid? assigneeId = null)
    {
        return ProjectTask.Create(
            project,
            "Write docs",
            null,
            ProjectTaskPriority.Medium,
            null,
            assigneeId
        );
    }

    [Fact]
    public void Create_ValidInput_StartsAsToDo()
    {
        var task = NewTask(NewProject());

        task.Status.Should().Be(ProjectTaskStatus.ToDo);
        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_AssigneeNotInProject_Throws()
    {
        var act = () => NewTask(NewProject(), Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*member*");
    }

    [Fact]
    public void Create_AssigneeInProject_SetsAssignee()
    {
        var task = NewTask(NewProject(), Member);

        task.AssigneeId.Should().Be(Member);
    }

    [Fact]
    public void ChangeStatus_ToCompleted_SetsCompletedAt()
    {
        var task = NewTask(NewProject());

        task.ChangeStatus(ProjectTaskStatus.Completed, Now);

        task.CompletedAt.Should().Be(Now);
    }

    [Fact]
    public void ChangeStatus_ReopenedAfterCompleted_ClearsCompletedAt()
    {
        var task = NewTask(NewProject());
        task.ChangeStatus(ProjectTaskStatus.Completed, Now);

        task.ChangeStatus(ProjectTaskStatus.InProgress, Now.AddDays(1));

        task.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void ChangeStatus_UndefinedValue_Throws()
    {
        var task = NewTask(NewProject());

        var act = () => task.ChangeStatus((ProjectTaskStatus)0, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Assign_Null_Unassigns()
    {
        var project = NewProject();
        var task = NewTask(project, Member);

        task.Assign(project, null);

        task.AssigneeId.Should().BeNull();
    }
}
