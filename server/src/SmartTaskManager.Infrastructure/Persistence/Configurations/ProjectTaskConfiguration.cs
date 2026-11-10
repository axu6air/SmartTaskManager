using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("ProjectTasks");

        builder.HasKey(t => t.Id).HasName("PK_ProjectTasks");
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.ProjectId).IsRequired();
        builder.Property(t => t.Title).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(4000);
        builder.Property(t => t.Status).IsRequired();
        builder.Property(t => t.Priority).IsRequired();
        builder.Property(t => t.IsAiModified).IsRequired();
        builder.Property(t => t.IsDeleted).IsRequired();
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder
            .HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .HasConstraintName("FK_ProjectTasks_Project")
            .OnDelete(DeleteBehavior.NoAction);

        // Composite FK: the assignee must be a member of this task's project.
        // It is not checked while AssigneeId is null, so unassigned tasks are allowed.
        builder
            .HasOne<ProjectMember>()
            .WithMany()
            .HasForeignKey(t => new { t.ProjectId, t.AssigneeId })
            .HasConstraintName("FK_ProjectTasks_Assignee")
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.DeletedBy)
            .HasConstraintName("FK_ProjectTasks_DeletedBy")
            .OnDelete(DeleteBehavior.NoAction);

        // Matches the Project filter, because every task belongs to a project.
        builder.HasQueryFilter(t => !t.IsDeleted && !t.Project.IsDeleted);

        // Project task lists and dashboard counts by status and priority.
        builder
            .HasIndex(t => new { t.ProjectId, t.Status })
            .IncludeProperties(t => new
            {
                t.Priority,
                t.DueDate,
                t.AssigneeId,
            })
            .HasFilter("IsDeleted = 0")
            .HasDatabaseName("IX_ProjectTasks_Project_Status");

        // "My tasks" and Assigned-scope checks.
        builder
            .HasIndex(t => new { t.AssigneeId, t.Status })
            .IncludeProperties(t => new
            {
                t.ProjectId,
                t.Priority,
                t.DueDate,
            })
            .HasFilter("IsDeleted = 0 AND AssigneeId IS NOT NULL")
            .HasDatabaseName("IX_ProjectTasks_Assignee_Status");

        // Dashboard "upcoming due" (open tasks only).
        builder
            .HasIndex(t => t.DueDate)
            .IncludeProperties(t => new
            {
                t.ProjectId,
                t.AssigneeId,
                t.Priority,
            })
            .HasFilter("IsDeleted = 0 AND Status IN (1, 2)")
            .HasDatabaseName("IX_ProjectTasks_DueDate_Open");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ProjectTasks_Status", "Status BETWEEN 1 AND 4");
            t.HasCheckConstraint("CK_ProjectTasks_Priority", "Priority BETWEEN 1 AND 4");
            t.HasCheckConstraint(
                "CK_ProjectTasks_Completed",
                "(Status = 3 AND CompletedAt IS NOT NULL) OR (Status <> 3 AND CompletedAt IS NULL)"
            );
            t.HasCheckConstraint(
                "CK_ProjectTasks_SoftDelete",
                "(IsDeleted = 0 AND DeletedAt IS NULL AND DeletedBy IS NULL) OR (IsDeleted = 1 AND DeletedAt IS NOT NULL)"
            );
        });

        builder.ConfigureAudit("ProjectTasks", createdByRequired: true);
    }
}
