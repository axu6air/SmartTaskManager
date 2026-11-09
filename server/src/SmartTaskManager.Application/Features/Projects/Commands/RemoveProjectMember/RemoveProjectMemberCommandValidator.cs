using FluentValidation;

namespace SmartTaskManager.Application.Features.Projects.Commands.RemoveProjectMember;

public sealed class RemoveProjectMemberCommandValidator : AbstractValidator<RemoveProjectMemberCommand>
{
    public RemoveProjectMemberCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmpty();
    }
}
