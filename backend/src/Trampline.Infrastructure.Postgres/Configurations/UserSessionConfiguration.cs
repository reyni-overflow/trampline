using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("sessions");

        builder.ComplexProperty(x => x.UserAgent, b =>
        {
            b.IsRequired();
            b.Property(a => a.Agent).HasColumnName("agent");
            b.Property(a => a.Ip).HasColumnName("location");
        });
    }
}