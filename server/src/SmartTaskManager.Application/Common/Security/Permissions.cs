namespace SmartTaskManager.Application.Common.Security;

// Codes must match dbo.Permissions.Code exactly.
public static class Permissions
{
    public static class Projects
    {
        public const string View = "projects.view";
        public const string Create = "projects.create";
        public const string Update = "projects.update";
        public const string Delete = "projects.delete";
        public const string ManageMembers = "projects.manage_members";
    }

    public static class Tasks
    {
        public const string View = "tasks.view";
        public const string Create = "tasks.create";
        public const string Update = "tasks.update";
        public const string UpdateStatus = "tasks.update_status";
        public const string Assign = "tasks.assign";
        public const string Delete = "tasks.delete";
    }

    public static class Users
    {
        public const string View = "users.view";
        public const string Manage = "users.manage";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Manage = "roles.manage";
    }

    public static class Dashboard
    {
        public const string View = "dashboard.view";
    }

    public static class Ai
    {
        public const string EnhanceDescription = "ai.enhance_description";
    }
}
