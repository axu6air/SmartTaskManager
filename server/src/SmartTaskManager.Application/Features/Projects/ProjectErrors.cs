using SmartTaskManager.Application.Common.Results;

namespace SmartTaskManager.Application.Features.Projects;

public static class ProjectErrors
{
    // Mirrors the unique index UX_Projects_Name_Active, so a clash is a 409 instead of a database error.
    public static readonly Error NameTaken = new(ErrorType.Conflict, "A project with this name already exists.");
}
