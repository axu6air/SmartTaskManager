using FluentValidation.Results;

namespace SmartTaskManager.Application.Common.Results;

public static class ValidationErrors
{
    // One validation Error with the messages grouped by property name.
    public static Error ToError(this IEnumerable<ValidationFailure> failures)
    {
        var details = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

        return new Error(ErrorType.Validation, "One or more validation errors occurred.", details);
    }
}
