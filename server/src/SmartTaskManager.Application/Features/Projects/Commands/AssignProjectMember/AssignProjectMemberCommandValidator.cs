using FluentValidation;

namespace SmartTaskManager.Application.Features.Projects.Commands.AssignProjectMember;

public sealed class AssignProjectMemberCommandValidator : AbstractValidator<AssignProjectMemberCommand>
{
    public AssignProjectMemberCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
    }
}
