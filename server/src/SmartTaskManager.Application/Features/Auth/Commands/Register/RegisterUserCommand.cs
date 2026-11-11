using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Auth.Commands.Register;

// Creates an account on behalf of someone else (users.manage); there is no self-registration.
// Without a RoleId the user gets the default role.
public sealed record RegisterUserCommand(string FullName, string Email, string Password, Guid? RoleId)
    : IRequest<Result<RegisterUserResponse>>;

public sealed record RegisterUserResponse(Guid Id, string FullName, string Email, IReadOnlyCollection<string> Roles);
