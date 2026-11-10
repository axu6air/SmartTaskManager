using FluentValidation;

namespace SmartTaskManager.Application.Features.Ai.Commands.EnhanceTaskDescription;

public sealed class EnhanceTaskDescriptionCommandValidator : AbstractValidator<EnhanceTaskDescriptionCommand>
{
    // Lengths match dbo.ProjectTasks, so the input is a description the user could save.
    public EnhanceTaskDescriptionCommandValidator()
    {
        RuleFor(c => c.Title).MaximumLength(200);
        RuleFor(c => c.Description).NotEmpty().MaximumLength(4000);
    }
}
