using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SmartTaskManager.Infrastructure.Persistence;

namespace SmartTaskManager.Infrastructure.UnitTests.Persistence;

public class ApplicationDbContextModelTests
{
    // Generating the create script needs no connection, but it forces EF to build and validate the model.
    private static string GenerateCreateScript()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Database=SmartTaskManagerModelTests;Trusted_Connection=True;")
            .Options;

        using var context = new ApplicationDbContext(options);
        return context.Database.GenerateCreateScript();
    }

    [Theory]
    [InlineData("Users")]
    [InlineData("Roles")]
    [InlineData("PermissionModules")]
    [InlineData("Permissions")]
    [InlineData("RolePermissions")]
    [InlineData("UserRoles")]
    [InlineData("RefreshTokens")]
    [InlineData("Projects")]
    [InlineData("ProjectMembers")]
    [InlineData("ProjectTasks")]
    [InlineData("AuditLogs")]
    public void Model_Built_MapsSchemaTable(string table)
    {
        GenerateCreateScript().Should().Contain($"CREATE TABLE [{table}]");
    }

    [Theory]
    [InlineData("FK_ProjectTasks_Assignee")]
    [InlineData("FK_RolePermissions_Role")]
    [InlineData("UX_Roles_SingleDefault")]
    [InlineData("UX_Projects_Name_Active")]
    [InlineData("CK_ProjectTasks_Completed")]
    [InlineData("CK_RolePermissions_Scope")]
    public void Model_Built_MapsSchemaConstraint(string name)
    {
        GenerateCreateScript().Should().Contain(name);
    }
}
