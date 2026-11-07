using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Common.Behaviors;

// Validates every command and query. Handlers can assume a valid request.
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IFailureResult<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = new List<ValidationFailure>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, ct);
            failures.AddRange(result.Errors);
        }

        return failures.Count == 0 ? await next() : TResponse.Failure(failures.ToError());
    }
}
