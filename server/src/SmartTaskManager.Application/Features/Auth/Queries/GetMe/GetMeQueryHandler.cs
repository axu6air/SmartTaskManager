using MediatR;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Auth.Queries.GetMe;

public sealed class GetMeQueryHandler(
    IUserRepository users,
    IPermissionService permissions,
    ICurrentUser currentUser
) : IRequestHandler<GetMeQuery, Result<MeResponse>>
{
    private static readonly Error Inactive = new(
        ErrorType.Unauthorized,
        "The account is not active."
    );

    public async Task<Result<MeResponse>> Handle(
        GetMeQuery request,
        CancellationToken cancellationToken
    )
    {
        var userId = currentUser.GetRequiredUserId();
        var user = await users.FindWithRolesAsync(userId, cancellationToken);

        if (user is null or { IsActive: false })
        {
            return Result<MeResponse>.Failure(Inactive);
        }

        var scopes = await permissions.GetAllScopesAsync(userId, cancellationToken);

        return Result<MeResponse>.Success(
            new MeResponse(
                user.Id,
                user.FullName,
                user.Email,
                user.UserRoles.Select(ur => ur.Role.Name).Order().ToList(),
                scopes
            )
        );
    }
}
