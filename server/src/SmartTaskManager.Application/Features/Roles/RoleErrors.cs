using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Roles;

public static class RoleErrors
{
    // System roles (Admin) keep their full grant set, so nobody can lock the workspace out of role management.
    public static readonly Error SystemRole = new(ErrorType.Conflict, "System roles cannot be edited.");

    public static readonly Error UnscopedNeedsAll = new(
        ErrorType.Validation,
        "This permission is not scopable, so it can only be granted with scope All."
    );
}
