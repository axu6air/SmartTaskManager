using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        builder.HasKey(p => p.Id).HasName("PK_Permissions");
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.ModuleId).IsRequired();
        builder.Property(p => p.Code).HasMaxLength(100).IsUnicode(false).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.IsScopable).IsRequired();
        builder.Property(p => p.DisplayOrder).IsRequired();

        builder
            .HasOne(p => p.Module)
            .WithMany()
            .HasForeignKey(p => p.ModuleId)
            .HasConstraintName("FK_Permissions_Module")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(p => p.Code).IsUnique().HasDatabaseName("UQ_Permissions_Code");
        builder
            .HasIndex(p => new { p.ModuleId, p.Name })
            .IsUnique()
            .HasDatabaseName("UQ_Permissions_Module_Name");
        builder.HasIndex(p => p.ModuleId).HasDatabaseName("IX_Permissions_ModuleId");

        builder.ConfigureAudit("Permissions");
    }
}
