using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Employee;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.HasOne(ja => ja.Profile)
            .WithMany(wp => wp.JobApplications)
            .HasForeignKey(ja => ja.WorkerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ja => ja.Job)
            .WithMany(j => j.JobApplications)
            .HasForeignKey(ja => ja.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(ja => new { ja.WorkerProfileId, ja.JobId })
            .IsUnique();

        builder.ToTable("jobApplications");
    }
}