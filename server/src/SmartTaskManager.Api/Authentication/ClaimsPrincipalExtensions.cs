using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace SmartTaskManager.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    // Null when the user is not authenticated or the token carries no valid user ID.
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        // "sub" is mapped to NameIdentifier by default, so accept either.
        var value = user.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (user.Identity?.IsAuthenticated == true && Guid.TryParse(value, out var userId))
        {
            return userId;
        }

        return null;
    }
}
