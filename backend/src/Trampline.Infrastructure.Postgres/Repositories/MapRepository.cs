using Microsoft.EntityFrameworkCore;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class MapRepository(AppDbContext context) : IMapRepository
{
    public async Task<IEnumerable<object>> GetMarkersAsync(CancellationToken ct)
    {
        var privateUserIds = await context.Users
            .AsNoTracking()
            .Where(u => u.IsPrivate)
            .Select(u => u.Id)
            .ToListAsync(ct);

        var jobs = await context.Jobs
            .AsNoTracking()
            .Where(j => j.IsActive && j.DeletedAt == null && !privateUserIds.Contains(j.UserId))
            .Select(j => new
            {
                Id = j.Id.ToString(),
                j.Title,
                Company = j.Employee.Name,
                Type = j.Type.ToString(),
                Lat = j.GeoLat,
                Lng = j.GeoLon,
                j.City,
                j.SalaryFrom,
                j.SalaryTo,
                Format = j.Format.ToString(),
                Tags = j.Tags.Select(t => t.Name).ToArray()
            })
            .ToListAsync(ct);

        var events = await context.Events
            .AsNoTracking()
            .Where(e => e.IsActive && e.DeletedAt == null && !privateUserIds.Contains(e.UserId))
            .Select(e => new
            {
                Id = e.Id.ToString(),
                e.Title,
                Company = e.Profile.Name,
                Type = "Event",
                Lat = e.GeoLat,
                Lng = e.GeoLon,
                e.City,
                e.SalaryFrom,
                e.SalaryTo,
                Format = e.Format.ToString(),
                Tags = e.Tags.Select(t => t.Name).ToArray()
            })
            .ToListAsync(ct);

        var mentorships = await context.Mentorships
            .AsNoTracking()
            .Where(m => m.IsActive && m.DeletedAt == null && !privateUserIds.Contains(m.UserId))
            .Select(m => new
            {
                Id = m.Id.ToString(),
                m.Title,
                Company = m.Profile.Name,
                Type = "Mentorship",
                Lat = m.GeoLat,
                Lng = m.GeoLon,
                m.City,
                m.SalaryFrom,
                m.SalaryTo,
                Format = m.Format.ToString(),
                Tags = m.Tags.Select(t => t.Name).ToArray()
            })
            .ToListAsync(ct);

        var markers = new List<object>(jobs.Count + events.Count + mentorships.Count);
        markers.AddRange(jobs);
        markers.AddRange(events);
        markers.AddRange(mentorships);

        return markers;
    }
}
