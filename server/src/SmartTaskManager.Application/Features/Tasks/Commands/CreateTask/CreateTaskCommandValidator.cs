using FluentValidation;

namespace SmartTaskManager.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandValidator : TaskFieldsValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator(TimeProvider timeProvider)
    {
        // Only on creation: an existing task may legitimately become overdue.
        RuleFor(c => c.DueDate)
            .Must(dueDate => dueDate >= timeProvider.GetUtcNow())
            .When(c => c.DueDate.HasValue)
            .WithMessage("The due date cannot be in the past.");
    }
}
