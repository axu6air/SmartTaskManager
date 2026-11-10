using System.Linq.Expressions;
using FluentAssertions;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Application.Features.Projects.Queries.GetProjects;
using SmartTaskManager.Application.Features.Tasks.Queries.GetTasks;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.UnitTests.Common;

public class ListQueryTests
{
    [Fact]
    public void Resolve_NoFieldOrDirection_UsesDefaultAscending()
    {
        var (sortBy, ascending) = ProjectSorts.Fields.Resolve(null, null);

        ascending.Should().BeTrue();
        sortBy.Should().BeSameAs(ProjectSorts.Fields.Resolve("name", "asc").SortBy);
    }

    [Fact]
    public void Resolve_FieldIsCaseInsensitive_AndDescIsHonoured()
    {
        var (_, ascending) = TaskSorts.Fields.Resolve("DUEDATE", "DESC");

        ascending.Should().BeFalse();
        TaskSorts.Fields.IsAllowed("DueDate").Should().BeTrue();
    }

    [Fact]
    public void Validator_UnknownSortField_IsRejected()
    {
        var result = new GetProjectsQueryValidator().Validate(new GetProjectsQuery(1, 10, SortBy: "passwordHash"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(GetProjectsQuery.SortBy));
    }

    [Fact]
    public void Validator_BadDirectionOrStatus_IsRejected()
    {
        var result = new GetTasksQueryValidator().Validate(
            new GetTasksQuery(Guid.NewGuid(), 1, 10, Status: (ProjectTaskStatus)9, SortDirection: "up"));

        result.Errors.Select(e => e.PropertyName).Should()
            .BeEquivalentTo([nameof(GetTasksQuery.Status), nameof(GetTasksQuery.SortDirection)]);
    }

    [Fact]
    public void Validator_ValidQuery_Passes()
    {
        var result = new GetTasksQueryValidator().Validate(
            new GetTasksQuery(Guid.NewGuid(), 2, 10, "invoice", ProjectTaskStatus.ToDo, ProjectTaskPriority.High,
                Guid.NewGuid(), "priority", "desc"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void And_CombinesBothConditionsOverOneParameter()
    {
        Expression<Func<int, bool>> positive = x => x > 0;
        Expression<Func<int, bool>> even = y => y % 2 == 0;

        var both = positive.And(even);

        both.Parameters.Should().ContainSingle();
        var compiled = both.Compile();
        compiled(4).Should().BeTrue();
        compiled(3).Should().BeFalse();
        compiled(-2).Should().BeFalse();
    }
}
