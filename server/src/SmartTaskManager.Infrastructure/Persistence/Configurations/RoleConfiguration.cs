using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id).HasName("PK_Roles");
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.Property(r => r.NormalizedName).HasMaxLength(100).IsRequired();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.IsSystem).IsRequired();
        builder.Property(r => r.IsDefault).IsRequired();
        builder.Property(r => r.RowVersion).IsRowVersion();

        builder
            .HasIndex(r => r.NormalizedName)
            .IsUnique()
            .HasDatabaseName("UQ_Roles_NormalizedName");

        // At most one default role.
        builder
            .HasIndex(r => r.IsDefault)
            .IsUnique()
            .HasFilter("IsDefault = 1")
            .HasDatabaseName("UX_Roles_SingleDefault");

        builder.ConfigureAudit("Roles");
    }
}
