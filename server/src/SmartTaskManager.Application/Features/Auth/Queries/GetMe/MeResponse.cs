using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Application.Features.Auth.Queries.GetMe;

// Permissions map each held code to its scopes, so a client can hide actions the API would refuse.
public sealed record MeResponse(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyDictionary<string, PermissionScopes> Permissions
);
