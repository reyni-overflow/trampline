using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Models.Worker;

namespace Trampline.Infrastructure.Postgres.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(x => x.Role)
            .HasConversion<string>();

        builder.Property(u => u.Phone).HasMaxLength(20);
        builder.HasIndex(u => u.Phone).IsUnique().HasFilter("\"Phone\" IS NOT NULL");

        builder.Property(u => u.IsBlocked).HasDefaultValue(false);

        builder.Property(u => u.TotpSecret).HasMaxLength(64);
        builder.Property(u => u.IsTotpEnabled).HasDefaultValue(false);
        builder.Property(u => u.MustChangePassword).HasDefaultValue(false);

        builder.HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.EmployeeProfile)
            .WithOne(x => x.User)
            .HasForeignKey<EmployeeProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.WorkerProfile)
            .WithOne(x => x.User)
            .HasForeignKey<WorkerProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(User.CreateSeed(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "admin@gmail.com",
            "Администратор",
            "3rljPMTphm1R5ozPxwb7ig==:W5meEqj/maKtTM0RxeTVlcs61YIPpPnj6yZu3Z0Eh2o=",
            Role.Admin,
            isSuperAdmin: true,
            mustChangePassword: true));

        builder.HasIndex(u => u.Email).IsUnique();

        builder.ToTable("users");
    }
}