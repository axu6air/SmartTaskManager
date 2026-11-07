namespace SmartTaskManager.Domain.Models.Identity;

public static class RefreshTokenRevokeReasons
{
    public const string Rotated = "Rotated";
    public const string Logout = "Logout";
    public const string ReuseDetected = "ReuseDetected";
    public const string RoleChanged = "RoleChanged";
}
