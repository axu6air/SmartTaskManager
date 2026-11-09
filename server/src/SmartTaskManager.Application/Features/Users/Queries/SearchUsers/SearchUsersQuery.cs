using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Users.Queries.SearchUsers;

// For pickers (adding project members): active users whose name or email contains the search text.
public sealed record SearchUsersQuery(string? Search) : IRequest<Result<IReadOnlyCollection<UserSummary>>>;

public sealed record UserSummary(Guid Id, string FullName, string Email);
