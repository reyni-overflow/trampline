using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("favorites");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Type).HasConversion<string>();

        builder.HasIndex(f => new { f.UserId, f.TargetId, f.Type }).IsUnique();
    }
}
