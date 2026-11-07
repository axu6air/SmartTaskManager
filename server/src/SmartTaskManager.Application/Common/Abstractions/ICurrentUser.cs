namespace SmartTaskManager.Application.Common.Abstractions;

public interface ICurrentUser
{
    // Null outside an authenticated request, for example during startup seeding.
    Guid? UserId { get; }

    Guid GetRequiredUserId();
}
