using FluentValidation;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Application.Features.Tasks;

// The editable fields of a task, shared by the create and update commands.
public interface ITaskFields
{
    string Title { get; }

    string? Description { get; }

    ProjectTaskPriority Priority { get; }

    DateTimeOffset? DueDate { get; }
}

public abstract class TaskFieldsValidator<T> : AbstractValidator<T>
    where T : ITaskFields
{
    // Lengths match dbo.ProjectTasks.
    protected TaskFieldsValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Description).MaximumLength(4000);
        RuleFor(c => c.Priority).IsInEnum();
    }
}
