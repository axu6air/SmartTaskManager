using FluentValidation;

namespace SmartTaskManager.Application.Features.Projects;

// The editable fields of a project, shared by the create and update commands.
public interface IProjectFields
{
    string Name { get; }

    string? Description { get; }

    DateTimeOffset? StartDate { get; }

    DateTimeOffset? EndDate { get; }
}

public abstract class ProjectFieldsValidator<T> : AbstractValidator<T>
    where T : IProjectFields
{
    // Lengths match dbo.Projects.
    protected ProjectFieldsValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Description).MaximumLength(2000);
        RuleFor(c => c.EndDate)
            .GreaterThanOrEqualTo(c => c.StartDate)
            .When(c => c.StartDate.HasValue && c.EndDate.HasValue)
            .WithMessage("The end date cannot be before the start date.");
    }
}
