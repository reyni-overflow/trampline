using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.AuthorName).HasMaxLength(200);

        builder.Property(r => r.AuthorRole).HasMaxLength(200);

        builder.Property(r => r.Text).HasMaxLength(2000);

        builder.Property(r => r.Rating).IsRequired();

        builder.HasIndex(r => r.UserId);
    }
}
