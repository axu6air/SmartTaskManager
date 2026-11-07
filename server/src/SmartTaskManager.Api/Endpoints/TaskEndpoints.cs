using SmartTaskManager.Api.Endpoints.Tasks.Assign;
using SmartTaskManager.Api.Endpoints.Tasks.Create;
using SmartTaskManager.Api.Endpoints.Tasks.Delete;
using SmartTaskManager.Api.Endpoints.Tasks.List;
using SmartTaskManager.Api.Endpoints.Tasks.Update;

namespace SmartTaskManager.Api.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectId:guid}/tasks").WithTags("Tasks");

        group.MapGetTasks();
        group.MapGetTask();
        group.MapCreateTask();
        group.MapUpdateTask();
        group.MapDeleteTask();
        group.MapAssignTask();

        return app;
    }
}
