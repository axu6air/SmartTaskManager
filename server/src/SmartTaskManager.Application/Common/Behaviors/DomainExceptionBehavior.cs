using MediatR;
using SmartTaskManager.Application.Common.Results;
using SmartTaskManager.Domain.Exceptions;

namespace SmartTaskManager.Application.Common.Behaviors;

// A broken business rule (for example "a project must keep at least one manager") becomes a 400, not a 500.
public sealed class DomainExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IFailureResult<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (DomainException exception)
        {
            return TResponse.Failure(new Error(ErrorType.Validation, exception.Message));
        }
    }
}
