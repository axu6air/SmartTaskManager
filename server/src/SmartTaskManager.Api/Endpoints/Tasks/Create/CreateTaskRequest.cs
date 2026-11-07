using FluentValidation;
using SmartTaskManager.Application.Features.Tasks;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Api.Endpoints.Tasks.Create;

// Priority is the number: 1 Low, 2 Medium, 3 High, 4 Critical.
public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    ProjectTaskPriority Priority,
    DateTimeOffset? DueDate,
    Guid? AssigneeId
) : ITaskFields;

public sealed class CreateTaskRequestValidator : TaskFieldsValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator(TimeProvider timeProvider)
    {
        // Only on creation: an existing task may legitimately become overdue.
        RuleFor(r => r.DueDate)
            .Must(dueDate => dueDate >= timeProvider.GetUtcNow())
            .When(r => r.DueDate.HasValue)
            .WithMessage("The due date cannot be in the past.");
    }
}
