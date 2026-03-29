using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Models.Worker;

namespace Trampline.Infrastructure.Postgres.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<UserSession> Sessions { get; set; }

    public DbSet<WorkerProfile> WorkerProfiles { get; set; }

    public DbSet<EmployeeProfile> EmployeeProfiles { get; set; }

    public DbSet<JobApplication> JobApplications { get; set; }

    public DbSet<EventApplication> EventApplications { get; set; }

    public DbSet<Job> Jobs { get; set; }

    public DbSet<Event> Events { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<Contact> Contacts { get; set; }

    public DbSet<Favorite> Favorites { get; set; }

    public DbSet<Recommendation> Recommendations { get; set; }

    public DbSet<Review> Reviews { get; set; }

    public DbSet<Notification> Notifications { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }

    public DbSet<Mentorship> Mentorships { get; set; }

    public DbSet<MentorshipApplication> MentorshipApplications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
    }
}