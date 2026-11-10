using FluentValidation;
using SmartTaskManager.Application.Common;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Projects.Queries.GetProjects;

public sealed class GetProjectsQueryValidator : AbstractValidator<GetProjectsQuery>
{
    public GetProjectsQueryValidator()
    {
        RuleFor(q => q.Search).MaximumLength(100);
        RuleFor(q => q.SortBy)
            .Must(ProjectSorts.Fields.IsAllowed)
            .WithMessage($"Sort by one of: {string.Join(", ", ProjectSorts.Fields.Names)}.");
        RuleFor(q => q.SortDirection)
            .Must(SortFields<Project>.IsDirection)
            .WithMessage("Sort direction must be asc or desc.");
    }
}
