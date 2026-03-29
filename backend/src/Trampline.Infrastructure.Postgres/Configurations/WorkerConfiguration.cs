using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Worker;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class WorkerConfiguration : IEntityTypeConfiguration<WorkerProfile>
{
    public void Configure(EntityTypeBuilder<WorkerProfile> builder)
    {
        builder.ComplexProperty(x => x.Info, b =>
        {
            b.IsRequired(false);
            b.Property(a => a.AdmissionAt).HasColumnName("admission");
            b.Property(a => a.Course).HasColumnName("course");
            b.Property(a => a.GraduationAt).HasColumnName("graduation");
            b.Property(a => a.University).HasColumnName("university");
        });

        builder.ToTable("workers");
    }
}