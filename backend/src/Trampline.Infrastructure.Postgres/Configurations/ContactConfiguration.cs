using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Worker;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Status).HasConversion<string>();

        builder.HasIndex(c => c.RequesterId);
        builder.HasIndex(c => c.ReceiverId);

        builder.HasIndex(c => new { c.RequesterId, c.ReceiverId }).IsUnique();
    }
}
