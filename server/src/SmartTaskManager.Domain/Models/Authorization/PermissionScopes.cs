namespace SmartTaskManager.Domain.Models.Authorization;

[Flags]
public enum PermissionScopes : byte
{
    Assigned = 1,
    Managed = 2,
    Member = 4,
    All = 8,
}
