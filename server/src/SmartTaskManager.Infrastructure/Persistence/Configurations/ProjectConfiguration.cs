using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Identity;
using SmartTaskManager.Domain.Models.Projects;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");

        builder.HasKey(p => p.Id).HasName("PK_Projects");
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.IsAiModified).IsRequired();
        builder.Property(p => p.IsDeleted).IsRequired();
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.DeletedBy)
            .HasConstraintName("FK_Projects_DeletedBy")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder
            .HasIndex(p => p.Name)
            .IsUnique()
            .HasFilter("IsDeleted = 0")
            .HasDatabaseName("UX_Projects_Name_Active");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Projects_Dates",
                "EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate"
            );
            t.HasCheckConstraint(
                "CK_Projects_SoftDelete",
                "(IsDeleted = 0 AND DeletedAt IS NULL AND DeletedBy IS NULL) OR (IsDeleted = 1 AND DeletedAt IS NOT NULL)"
            );
        });

        builder.ConfigureAudit("Projects", createdByRequired: true);
    }
}
