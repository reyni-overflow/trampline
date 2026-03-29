using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Employee;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class EventApplicationConfiguration : IEntityTypeConfiguration<EventApplication>
{
    public void Configure(EntityTypeBuilder<EventApplication> builder)
    {
        builder.HasOne(ea => ea.Profile)
            .WithMany(wp => wp.EventApplications)
            .HasForeignKey(ea => ea.WorkerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ea => ea.Event)
            .WithMany(e => e.EventApplications)
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ea => new { ea.WorkerProfileId, ea.EventId })
            .IsUnique();

        builder.HasIndex(ea => ea.WorkerProfileId);

        builder.ToTable("eventApplications");
    }
}
