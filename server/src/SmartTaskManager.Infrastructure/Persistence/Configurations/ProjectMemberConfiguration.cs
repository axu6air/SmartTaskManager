using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("ProjectMembers");

        builder.HasKey(m => new { m.ProjectId, m.UserId }).HasName("PK_ProjectMembers");

        builder.Property(m => m.IsManager).IsRequired();

        // Matches the Project filter, so a deleted project's members are hidden with it.
        builder.HasQueryFilter(m => !m.Project.IsDeleted);

        builder
            .HasOne(m => m.Project)
            .WithMany(p => p.Members)
            .HasForeignKey(m => m.ProjectId)
            .HasConstraintName("FK_ProjectMembers_Project")
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .HasConstraintName("FK_ProjectMembers_User")
            .OnDelete(DeleteBehavior.NoAction);

        // "Which projects am I in / do I manage" (scope checks, project lists).
        builder
            .HasIndex(m => m.UserId)
            .IncludeProperties(m => m.IsManager)
            .HasDatabaseName("IX_ProjectMembers_UserId");

        builder.ConfigureAudit("ProjectMembers", createdByRequired: true);
    }
}
