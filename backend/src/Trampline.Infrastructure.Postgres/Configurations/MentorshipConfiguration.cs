using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Employee;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class MentorshipConfiguration : IEntityTypeConfiguration<Mentorship>
{
    public void Configure(EntityTypeBuilder<Mentorship> builder)
    {
        builder.Property(x => x.Format)
            .HasConversion<string>();

        builder.HasMany(x => x.Tags)
            .WithMany(x => x.Mentorships)
            .UsingEntity(j => j.ToTable("mentorshipTags"));

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.CreatedAt);

        builder.ToTable("mentorships");
    }
}
