using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Domain.Models.Identity;

public class RefreshToken : Entity, ITrackCreated, ITrackUpdated
{
    private RefreshToken() { }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public Guid FamilyId { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? RevokedByIp { get; private set; }

    public string? RevokeReason { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        Guid familyId,
        DateTimeOffset expiresAt,
        string? createdByIp
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);

        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            FamilyId = familyId,
            ExpiresAt = expiresAt,
            CreatedByIp = createdByIp,
        };
    }

    public bool IsActive(DateTimeOffset now) => !IsRevoked && ExpiresAt > now;

    public void Revoke(
        DateTimeOffset now,
        string reason,
        string? revokedByIp,
        Guid? replacedByTokenId = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        if (IsRevoked)
        {
            throw new DomainException("The refresh token is already revoked.");
        }

        RevokedAt = now;
        RevokeReason = reason;
        RevokedByIp = revokedByIp;
        ReplacedByTokenId = replacedByTokenId;
    }
}
