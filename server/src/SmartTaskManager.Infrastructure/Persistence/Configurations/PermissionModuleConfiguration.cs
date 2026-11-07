using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class PermissionModuleConfiguration : IEntityTypeConfiguration<PermissionModule>
{
    public void Configure(EntityTypeBuilder<PermissionModule> builder)
    {
        builder.ToTable("PermissionModules");

        builder.HasKey(m => m.Id).HasName("PK_PermissionModules");
        builder.Property(m => m.Id).ValueGeneratedOnAdd();

        builder.Property(m => m.Code).HasMaxLength(50).IsUnicode(false).IsRequired();
        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.DisplayOrder).IsRequired();

        builder.HasIndex(m => m.Code).IsUnique().HasDatabaseName("UQ_PermissionModules_Code");
        builder.HasIndex(m => m.Name).IsUnique().HasDatabaseName("UQ_PermissionModules_Name");

        builder.ConfigureAudit("PermissionModules");
    }
}
