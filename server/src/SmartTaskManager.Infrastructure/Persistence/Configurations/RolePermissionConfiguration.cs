using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Authorization;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");

        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId }).HasName("PK_RolePermissions");

        builder.Property(rp => rp.Scope).IsRequired();

        builder
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .HasConstraintName("FK_RolePermissions_Role")
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(rp => rp.Permission)
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .HasConstraintName("FK_RolePermissions_Permission")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(rp => rp.PermissionId).HasDatabaseName("IX_RolePermissions_PermissionId");

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_RolePermissions_Scope", "Scope BETWEEN 1 AND 15")
        );

        builder.ConfigureAudit("RolePermissions");
    }
}
