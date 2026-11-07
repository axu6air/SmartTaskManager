namespace SmartTaskManager.Api.Authorization;

public static class EndpointAuthorizationExtensions
{
    public static TBuilder Authenticate<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
        => builder.WithMetadata(new AuthenticationRequirement());

    // The code is "<module>.<permission>", e.g. Permissions.Projects.Create. Implies Authenticate().
    public static TBuilder Authorize<TBuilder>(this TBuilder builder, string permissionCode)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);
        return builder.WithMetadata(new PermissionRequirement(permissionCode));
    }

    public static IApplicationBuilder UseEndpointAuthorization(this IApplicationBuilder app)
        => app.UseMiddleware<EndpointAuthorizationMiddleware>();
}
