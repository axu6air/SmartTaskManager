using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Authorization;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Auth.Commands.Register;

public sealed class RegisterUserCommandHandler(
    IUserRepository users,
    IRoleRepository roles,
    IPasswordHasher passwordHasher
) : IRequestHandler<RegisterUserCommand, Result<RegisterUserResponse>>
{
    private static readonly Error EmailTaken = new(ErrorType.Conflict, "A user with this email already exists.");

    private static readonly Error NoDefaultRole = new(
        ErrorType.Conflict,
        "No default role is configured. Choose a role for the new user."
    );

    public async Task<Result<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var normalizedEmail = User.NormalizeEmail(request.Email);
        if (await users.ExistsAsync(u => u.NormalizedEmail == normalizedEmail, ct))
        {
            return Result<RegisterUserResponse>.Failure(EmailTaken);
        }

        var role = await FindRoleAsync(request.RoleId, ct);
        if (!role.IsSuccess)
        {
            return Result<RegisterUserResponse>.Failure(role.Error!);
        }

        var user = User.Create(request.FullName, request.Email, passwordHasher.Hash(request.Password));
        user.AssignRole(role.Value!);
        await users.CreateAsync(user, ct);

        return Result<RegisterUserResponse>.Success(
            new RegisterUserResponse(user.Id, user.FullName, user.Email, [role.Value!.Name])
        );
    }

    // Tracked, so adding the user does not make EF try to insert the role as well.
    private async Task<Result<Role>> FindRoleAsync(Guid? roleId, CancellationToken ct)
    {
        if (roleId is { } id)
        {
            var chosen = await roles.FindAsync(id, tracking: true, ct: ct);
            return chosen is null ? Result<Role>.Failure(Errors.NotFound("Role")) : Result<Role>.Success(chosen);
        }

        var fallback = await roles.FindAsync(r => r.IsDefault, tracking: true, ct: ct);
        return fallback is null ? Result<Role>.Failure(NoDefaultRole) : Result<Role>.Success(fallback);
    }
}
