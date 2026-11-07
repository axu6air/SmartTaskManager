using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain.Models.Auditing;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id).HasName("PK_AuditLogs");
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.Action).HasMaxLength(100).IsUnicode(false).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100).IsUnicode(false).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(100).IsUnicode(false).IsRequired();
        builder.Property(a => a.IpAddress).HasMaxLength(45).IsUnicode(false);

        builder
            .HasIndex(a => new
            {
                a.EntityType,
                a.EntityId,
                a.CreatedAt,
            })
            .HasDatabaseName("IX_AuditLogs_Entity");

        builder.ConfigureAudit("AuditLogs");
    }
}
