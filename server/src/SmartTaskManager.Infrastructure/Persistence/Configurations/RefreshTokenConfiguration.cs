using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Identity;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id).HasName("PK_RefreshTokens");
        builder.Property(t => t.Id).ValueGeneratedOnAdd();

        builder.Property(t => t.UserId).IsRequired();
        builder
            .Property(t => t.TokenHash)
            .HasMaxLength(64)
            .IsFixedLength()
            .IsUnicode(false)
            .IsRequired();
        builder.Property(t => t.FamilyId).IsRequired();
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.CreatedByIp).HasMaxLength(45).IsUnicode(false);
        builder.Property(t => t.RevokedByIp).HasMaxLength(45).IsUnicode(false);
        builder.Property(t => t.RevokeReason).HasMaxLength(100);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .HasConstraintName("FK_RefreshTokens_User")
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<RefreshToken>()
            .WithMany()
            .HasForeignKey(t => t.ReplacedByTokenId)
            .HasConstraintName("FK_RefreshTokens_ReplacedBy")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(t => t.TokenHash).IsUnique().HasDatabaseName("UQ_RefreshTokens_TokenHash");

        builder
            .HasIndex(t => t.UserId)
            .HasFilter("RevokedAt IS NULL")
            .HasDatabaseName("IX_RefreshTokens_UserId_Active");

        builder.HasIndex(t => t.FamilyId).HasDatabaseName("IX_RefreshTokens_FamilyId");

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_RefreshTokens_Expiry", "ExpiresAt > CreatedAt")
        );

        builder.ConfigureAudit("RefreshTokens");
    }
}
