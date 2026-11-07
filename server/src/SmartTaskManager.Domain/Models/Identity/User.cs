using SmartTaskManager.Domain.Exceptions;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Domain.Models.Identity;

public class User : Entity, ITrackCreated, ITrackUpdated
{
    private readonly List<UserRole> _userRoles = [];

    private User() { }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string NormalizedEmail { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public Guid SecurityStamp { get; private set; }

    public bool IsActive { get; private set; }

    public int AccessFailedCount { get; private set; }

    public DateTimeOffset? LockoutEnd { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public byte[] RowVersion { get; private set; } = [];

    public static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();

    public static User Create(string fullName, string email, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim(),
            NormalizedEmail = NormalizeEmail(email),
            PasswordHash = passwordHash,
            SecurityStamp = Guid.NewGuid(),
            IsActive = true,
        };
    }

    public void UpdateProfile(string fullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        FullName = fullName.Trim();
    }

    public void ChangePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        SecurityStamp = Guid.NewGuid();
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        IsActive = false;
        SecurityStamp = Guid.NewGuid();
    }

    public bool IsLockedOut(DateTimeOffset now) => LockoutEnd.HasValue && LockoutEnd.Value > now;

    public void RecordSuccessfulLogin(DateTimeOffset now)
    {
        LastLoginAt = now;
        AccessFailedCount = 0;
        LockoutEnd = null;
    }

    public void RecordFailedLogin(
        DateTimeOffset now,
        int maxFailedAttempts,
        TimeSpan lockoutDuration
    )
    {
        AccessFailedCount++;

        if (AccessFailedCount >= maxFailedAttempts)
        {
            LockoutEnd = now.Add(lockoutDuration);
            AccessFailedCount = 0;
        }
    }

    // UserRoles must be loaded before calling AssignRole or RemoveRole.
    public void AssignRole(Role role)
    {
        ArgumentNullException.ThrowIfNull(role);

        if (_userRoles.Any(ur => ur.RoleId == role.Id))
        {
            throw new DomainException("The user already has this role.");
        }

        _userRoles.Add(new UserRole(this, role));
    }

    public void RemoveRole(Guid roleId)
    {
        var existing =
            _userRoles.FirstOrDefault(ur => ur.RoleId == roleId)
            ?? throw new DomainException("The user does not have this role.");

        _userRoles.Remove(existing);
    }
}
