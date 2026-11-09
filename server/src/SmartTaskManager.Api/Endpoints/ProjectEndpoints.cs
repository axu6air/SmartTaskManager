using SmartTaskManager.Api.Endpoints.Projects.Assign;
using SmartTaskManager.Api.Endpoints.Projects.Create;
using SmartTaskManager.Api.Endpoints.Projects.Delete;
using SmartTaskManager.Api.Endpoints.Projects.List;
using SmartTaskManager.Api.Endpoints.Projects.Members;
using SmartTaskManager.Api.Endpoints.Projects.Update;

namespace SmartTaskManager.Api.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        group.MapGetProjects();
        group.MapGetProject();
        group.MapCreateProject();
        group.MapUpdateProject();
        group.MapDeleteProject();
        group.MapAssignProjectMember();
        group.MapGetProjectMembers();
        group.MapRemoveProjectMember();

        return app;
    }
}
