using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Identity;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id).HasName("PK_Users");
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(u => u.FullName).HasMaxLength(150).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.SecurityStamp).IsRequired();
        builder.Property(u => u.IsActive).IsRequired();
        builder.Property(u => u.AccessFailedCount).IsRequired();
        builder.Property(u => u.RowVersion).IsRowVersion();

        builder
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("UQ_Users_NormalizedEmail");

        builder.ConfigureAudit("Users");
    }
}
