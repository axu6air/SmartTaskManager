using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartTaskManager.Domain;
using SmartTaskManager.Domain.Models.Identity;

namespace SmartTaskManager.Infrastructure.Persistence.Configurations;

internal static class AuditConfigurationExtensions
{
    // Every table carries the same four audit columns and two foreign keys to Users.
    public static void ConfigureAudit<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string table,
        bool createdByRequired = false
    )
        where TEntity : class, ITrackCreated, ITrackUpdated
    {
        builder.Property(nameof(ITrackCreated.CreatedAt)).IsRequired();
        builder.Property(nameof(ITrackCreated.CreatedBy)).IsRequired(createdByRequired);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(nameof(ITrackCreated.CreatedBy))
            .HasConstraintName($"FK_{table}_CreatedBy")
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(nameof(ITrackUpdated.UpdatedBy))
            .HasConstraintName($"FK_{table}_UpdatedBy")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
