using SmartTaskManager.Api;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Endpoints;
using SmartTaskManager.Api.Middleware;
using SmartTaskManager.Application;
using SmartTaskManager.Infrastructure;
using SmartTaskManager.Infrastructure.Persistence.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder
    .Services.AddRequestContext()
    .AddApplication()
    .AddInfrastructure()
    .AddJwtAuthentication()
    .AddValidators();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection().UseAuthentication().UseRequestLogging().UseEndpointAuthorization();
app.MapEndpoints();

await app.RunAsync();
