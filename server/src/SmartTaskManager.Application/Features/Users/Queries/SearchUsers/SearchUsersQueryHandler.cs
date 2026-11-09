using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Repositories;

namespace SmartTaskManager.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryHandler(IUserRepository users)
    : IRequestHandler<SearchUsersQuery, Result<IReadOnlyCollection<UserSummary>>>
{
    private const int MaxResults = 20;

    public async Task<Result<IReadOnlyCollection<UserSummary>>> Handle(SearchUsersQuery request, CancellationToken ct)
    {
        var search = request.Search?.Trim() ?? string.Empty;
        var normalized = User.NormalizeEmail(search);

        // A picker shows the best few matches; the user narrows the search rather than paging.
        var page = await users.FindResultSetAsync(
            page: 1,
            pageSize: MaxResults,
            predicate: u => u.IsActive
                && (search == string.Empty || u.FullName.Contains(search) || u.NormalizedEmail.Contains(normalized)),
            sortBy: u => u.FullName,
            ct: ct
        );

        return Result<IReadOnlyCollection<UserSummary>>.Success(
            page.Data.Select(u => new UserSummary(u.Id, u.FullName, u.Email)).ToList()
        );
    }
}
