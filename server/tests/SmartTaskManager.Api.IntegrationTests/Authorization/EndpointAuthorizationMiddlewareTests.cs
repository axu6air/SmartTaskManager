using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Application.Common.Abstractions;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Api.IntegrationTests.Authorization;

public class EndpointAuthorizationMiddlewareTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private sealed class StubPermissions(PermissionScopes? scopes) : IPermissionService
    {
        public Task<PermissionScopes?> GetScopesAsync(Guid userId, string permissionCode, CancellationToken ct)
            => Task.FromResult(scopes);
    }

    private static async Task<(int Status, bool NextCalled)> RunAsync(
        object? requirement, bool authenticated, PermissionScopes? scopes)
    {
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(
            null, new EndpointMetadataCollection(requirement is null ? [] : [requirement]), "test"));
        if (authenticated)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, UserId.ToString())], "test"));
        }

        var nextCalled = false;
        var middleware = new EndpointAuthorizationMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, new StubPermissions(scopes));

        return (context.Response.StatusCode, nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_NoRequirement_PassesThrough()
    {
        var (_, nextCalled) = await RunAsync(null, authenticated: false, scopes: null);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AuthenticateWithoutUser_Returns401()
    {
        var (status, nextCalled) = await RunAsync(new AuthenticationRequirement(), authenticated: false, scopes: null);

        status.Should().Be(401);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_AuthenticateWithUser_PassesThrough()
    {
        var (_, nextCalled) = await RunAsync(new AuthenticationRequirement(), authenticated: true, scopes: null);

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_PermissionWithoutUser_Returns401()
    {
        var (status, _) = await RunAsync(new PermissionRequirement("projects.create"), authenticated: false, scopes: PermissionScopes.All);

        status.Should().Be(401);
    }

    [Fact]
    public async Task InvokeAsync_PermissionNotGranted_Returns403()
    {
        var (status, nextCalled) = await RunAsync(new PermissionRequirement("projects.create"), authenticated: true, scopes: null);

        status.Should().Be(403);
        nextCalled.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_PermissionGranted_PassesThrough()
    {
        var (_, nextCalled) = await RunAsync(new PermissionRequirement("projects.create"), authenticated: true, scopes: PermissionScopes.Managed);

        nextCalled.Should().BeTrue();
    }
}
