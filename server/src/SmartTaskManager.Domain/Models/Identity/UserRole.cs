using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Domain.Models.Identity;

public class UserRole : ITrackCreated, ITrackUpdated
{
    private UserRole() { }

    internal UserRole(User user, Role role)
    {
        User = user;
        UserId = user.Id;
        Role = role;
        RoleId = role.Id;
    }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}
