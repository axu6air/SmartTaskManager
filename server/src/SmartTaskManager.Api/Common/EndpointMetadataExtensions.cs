namespace SmartTaskManager.Api.Common;

public static class EndpointMetadataExtensions
{
    // Documents the error responses an endpoint can return, all of them in the ApiResponse envelope.
    public static RouteHandlerBuilder ProducesErrors(this RouteHandlerBuilder builder, params int[] statusCodes)
    {
        foreach (var statusCode in statusCodes)
        {
            builder.Produces<ApiResponse<object>>(statusCode);
        }

        return builder;
    }
}
