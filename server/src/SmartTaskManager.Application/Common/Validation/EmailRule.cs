using FluentValidation;

namespace SmartTaskManager.Application.Common.Validation;

// The one email format rule; the Angular client uses the same pattern (shared/validators/email.ts).
public static class EmailRule
{
    // local@domain.tld: no spaces, exactly one @, a dot in the domain, an ending of 2+ letters.
    // Stricter than FluentValidation's EmailAddress(), which accepts "name@company".
    public const string Pattern = @"^[^\s@]+@[^\s@]+\.[A-Za-z]{2,}$";

    public const string Message = "Enter a valid email address, like name@company.com.";

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.Matches(Pattern).WithMessage(Message);
}
