using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Identity;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId }).HasName("PK_UserRoles");

        // No cascade from Roles: a role that is still assigned to users cannot be deleted.
        builder
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .HasConstraintName("FK_UserRoles_User")
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .HasConstraintName("FK_UserRoles_Role")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(ur => ur.RoleId).HasDatabaseName("IX_UserRoles_RoleId");

        builder.ConfigureAudit("UserRoles");
    }
}
