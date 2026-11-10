using SmartTaskManager.Api;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Cors;
using SmartTaskManager.Api.Endpoints;
using SmartTaskManager.Api.HealthChecks;
using SmartTaskManager.Api.Logging;
using SmartTaskManager.Api.Middleware;
using SmartTaskManager.Api.RateLimiting;
using SmartTaskManager.Application;
using SmartTaskManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.AddSerilogLogging();

// Add services to the container.
builder
    .Services.AddRequestContext()
    .AddApplication()
    .AddInfrastructure()
    .AddExceptionHandling()
    .AddJwtAuthentication()
    .AddValidators()
    .AddCorsPolicy()
    .AddRateLimiting();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    await app.Services.InitializeDatabaseAsync();
//}

// Configure the HTTP request pipeline.

// CORS runs before authentication so preflight requests are answered without a token.
// Rate limiting runs after authentication (to count per user) and before the permission check.
app.UseExceptionHandler()
    .UseHttpsRedirection()
    .UseCors()
    .UseAuthentication()
    .UseRequestLogging()
    .UseRateLimiter()
    .UseEndpointAuthorization();
app.MapHealthEndpoints();
app.MapEndpoints();

await app.RunAsync();
