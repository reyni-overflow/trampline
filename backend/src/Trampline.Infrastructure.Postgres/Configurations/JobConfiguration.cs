using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Employee;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.Property(x => x.Format)
            .HasConversion<string>();

        builder.Property(x => x.Type)
            .HasConversion<string>();

        builder.HasMany(x => x.Tags)
            .WithMany(x => x.Jobs)
            .UsingEntity(j => j.ToTable("jobTags"));

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.CreatedAt);

        builder.ToTable("jobs");
    }
}