using FluentValidation;
using SmartTaskManager.Api.Common;
using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Api;

public sealed class ValidationFilter<T> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
            return await next(context);

        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
            return await next(context);

        var validationResult = await validator.ValidateAsync(
            model,
            context.HttpContext.RequestAborted
        );

        if (!validationResult.IsValid)
        {
            // Same 400 body as a validation failure from the MediatR pipeline.
            return Result<object>
                .Failure(validationResult.Errors.ToError())
                .ToHttpResult(context.HttpContext);
        }

        return await next(context);
    }
}
