using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.Id);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.EntityType);
        builder.HasIndex(a => a.CreatedAt);

        builder.Property(a => a.UserName).HasMaxLength(200);
        builder.Property(a => a.UserRole).HasMaxLength(100);
        builder.Property(a => a.Action).HasMaxLength(200);
        builder.Property(a => a.EntityType).HasMaxLength(200);
        builder.Property(a => a.Details).HasMaxLength(2000);
        builder.Property(a => a.IpAddress).HasMaxLength(100);
    }
}
