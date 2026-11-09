using FluentValidation;

namespace SmartTaskManager.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersQueryValidator()
    {
        RuleFor(q => q.Search).MaximumLength(100);
    }
}
