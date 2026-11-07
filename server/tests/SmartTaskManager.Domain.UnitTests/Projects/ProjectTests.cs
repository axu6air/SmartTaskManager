using FluentAssertions;
using SmartTaskManager.Domain.Exceptions;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Domain.UnitTests.Projects;

public class ProjectTests
{
    private static readonly Guid Creator = Guid.NewGuid();

    private static Project NewProject() => Project.Create("Apollo", null, null, null, Creator);

    [Fact]
    public void Create_ValidInput_AddsCreatorAsManager()
    {
        var project = NewProject();

        project.Members.Should().ContainSingle(m => m.UserId == Creator && m.IsManager);
    }

    [Fact]
    public void Create_EndDateBeforeStartDate_Throws()
    {
        var start = DateTimeOffset.UtcNow;

        var act = () => Project.Create("Apollo", null, start, start.AddDays(-1), Creator);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveMember_LastManager_Throws()
    {
        var project = NewProject();

        var act = () => project.RemoveMember(Creator);

        act.Should().Throw<DomainException>().WithMessage("*at least one manager*");
    }

    [Fact]
    public void SetManager_DemotingLastManager_Throws()
    {
        var project = NewProject();

        var act = () => project.SetManager(Creator, isManager: false);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveMember_ManagerWhenAnotherExists_RemovesMember()
    {
        var project = NewProject();
        var second = Guid.NewGuid();
        project.AddMember(second, isManager: true);

        project.RemoveMember(Creator);

        project.IsMember(Creator).Should().BeFalse();
        project.IsManager(second).Should().BeTrue();
    }

    [Fact]
    public void AddMember_ExistingMember_Throws()
    {
        var project = NewProject();

        var act = () => project.AddMember(Creator);

        act.Should().Throw<DomainException>();
    }
}
