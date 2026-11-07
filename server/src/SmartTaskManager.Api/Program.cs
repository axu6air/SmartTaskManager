using SmartTaskManager.Api.Authentication;
using SmartTaskManager.Api.Authorization;
using SmartTaskManager.Api.Endpoints;
using SmartTaskManager.Application;
using SmartTaskManager.Infrastructure;
using SmartTaskManager.Infrastructure.Persistence.Initialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddJwtAuthentication();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.InitializeDatabaseAsync();
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseEndpointAuthorization();

app.MapAuthEndpoints();

await app.RunAsync();
