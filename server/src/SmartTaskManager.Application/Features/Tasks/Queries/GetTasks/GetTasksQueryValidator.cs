using FluentValidation;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Tasks.Queries.GetTasks;

public sealed class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    public GetTasksQueryValidator()
    {
        RuleFor(q => q.Search).MaximumLength(100);
        RuleFor(q => q.Status).IsInEnum().When(q => q.Status.HasValue);
        RuleFor(q => q.Priority).IsInEnum().When(q => q.Priority.HasValue);
        RuleFor(q => q.SortBy)
            .Must(TaskSorts.Fields.IsAllowed)
            .WithMessage($"Sort by one of: {string.Join(", ", TaskSorts.Fields.Names)}.");
        RuleFor(q => q.SortDirection)
            .Must(SortFields<ProjectTask>.IsDirection)
            .WithMessage("Sort direction must be asc or desc.");
    }
}
