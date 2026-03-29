using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.Property(n => n.Type).HasMaxLength(100);
        builder.Property(n => n.Title).HasMaxLength(500);
        builder.Property(n => n.Message).HasMaxLength(2000);
        builder.Property(n => n.Link).HasMaxLength(500);
    }
}
