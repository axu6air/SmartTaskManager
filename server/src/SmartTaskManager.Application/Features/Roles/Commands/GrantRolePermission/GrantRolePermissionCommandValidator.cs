using FluentValidation;

namespace SmartTaskManager.Application.Features.Roles.Commands.GrantRolePermission;

public sealed class GrantRolePermissionCommandValidator : AbstractValidator<GrantRolePermissionCommand>
{
    // Scope is a flags value: any combination of 1, 2, 4 and 8.
    public GrantRolePermissionCommandValidator()
    {
        RuleFor(c => (byte)c.Scope).InclusiveBetween((byte)1, (byte)15).WithName("Scope");
    }
}
