using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Employee;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class MentorshipApplicationConfiguration : IEntityTypeConfiguration<MentorshipApplication>
{
    public void Configure(EntityTypeBuilder<MentorshipApplication> builder)
    {
        builder.HasOne(ma => ma.Profile)
            .WithMany(wp => wp.MentorshipApplications)
            .HasForeignKey(ma => ma.WorkerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ma => ma.Mentorship)
            .WithMany(m => m.MentorshipApplications)
            .HasForeignKey(ma => ma.MentorshipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ma => new { ma.WorkerProfileId, ma.MentorshipId })
            .IsUnique();

        builder.HasIndex(ma => ma.WorkerProfileId);

        builder.ToTable("mentorshipApplications");
    }
}
