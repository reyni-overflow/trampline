using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Employee;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.Property(x => x.Format)
            .HasConversion<string>();

        builder.HasMany(x => x.Tags)
            .WithMany(x => x.Events);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.CreatedAt);

        builder.ToTable("events");
    }
}