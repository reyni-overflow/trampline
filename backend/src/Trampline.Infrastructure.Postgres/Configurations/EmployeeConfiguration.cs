using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models.Employee;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
    {
        builder.ComplexProperty(x => x.Info, b =>
        {
            b.IsRequired();
            b.Property(a => a.Address).HasColumnName("address");
            b.Property(a => a.Email).HasColumnName("email");
            b.Property(a => a.INN).HasColumnName("inn");
        });

        builder.Ignore(x => x.IsVerified);
        builder.Ignore(x => x.IsTrusted);

        builder.ToTable("employees");
    }
}