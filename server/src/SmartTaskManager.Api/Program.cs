using SmartTaskManager.Api;
using SmartTaskManager.Api.Authentication;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Endpoints;
using SmartTaskManager.Api.Middleware;
using SmartTaskManager.Application;
using SmartTaskManager.Infrastructure;
using SmartTaskManager.Infrastructure.Persistence.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddJwtAuthentication();
builder.Services.AddValidators();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRequestLogging();
app.UseEndpointAuthorization();

app.MapAuthEndpoints();
app.MapProjectEndpoints();
app.MapTaskEndpoints();
app.MapRoleEndpoints();
app.MapUserEndpoints();

await app.RunAsync();
