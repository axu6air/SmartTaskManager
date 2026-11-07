using SmartTaskManager.Application.Common.Abstractions;

namespace SmartTaskManager.Api.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId => httpContextAccessor.HttpContext?.User.GetUserId();

    public Guid GetRequiredUserId() =>
        UserId ?? throw new InvalidOperationException("There is no authenticated user.");
}
