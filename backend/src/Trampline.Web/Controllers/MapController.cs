using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Web.Controllers;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class MapController(AppDbContext dbContext) : ControllerBase
{
    public record MapMarkerResponse(
        string Id,
        string Title,
        string Company,
        string Type,
        double Lat,
        double Lng,
        string? City,
        decimal? SalaryFrom,
        decimal? SalaryTo,
        string Format,
        string[] Tags);

    [AllowAnonymous]
    [SwaggerOperation(Summary = "Маркеры для карты", Description = "Лёгкий endpoint — только поля для маркеров")]
    [HttpGet("markers")]
    public async Task<IActionResult> GetMarkersAsync(CancellationToken ct)
    {
        var privateUserIds = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.IsPrivate)
            .Select(u => u.Id)
            .ToListAsync(ct);

        var jobs = await dbContext.Jobs
            .AsNoTracking()
            .Where(j => j.IsActive && j.DeletedAt == null && !privateUserIds.Contains(j.UserId))
            .Select(j => new MapMarkerResponse(
                j.Id.ToString(),
                j.Title,
                j.Employee.Name,
                j.Type.ToString(),
                j.GeoLat,
                j.GeoLon,
                j.City,
                j.SalaryFrom,
                j.SalaryTo,
                j.Format.ToString(),
                j.Tags.Select(t => t.Name).ToArray()))
            .ToListAsync(ct);

        var events = await dbContext.Events
            .AsNoTracking()
            .Where(e => e.IsActive && e.DeletedAt == null && !privateUserIds.Contains(e.UserId))
            .Select(e => new MapMarkerResponse(
                e.Id.ToString(),
                e.Title,
                e.Profile.Name,
                "Event",
                e.GeoLat,
                e.GeoLon,
                e.City,
                e.SalaryFrom,
                e.SalaryTo,
                e.Format.ToString(),
                e.Tags.Select(t => t.Name).ToArray()))
            .ToListAsync(ct);

        var mentorships = await dbContext.Mentorships
            .AsNoTracking()
            .Where(m => m.IsActive && m.DeletedAt == null && !privateUserIds.Contains(m.UserId))
            .Select(m => new MapMarkerResponse(
                m.Id.ToString(),
                m.Title,
                m.Profile.Name,
                "Mentorship",
                m.GeoLat,
                m.GeoLon,
                m.City,
                m.SalaryFrom,
                m.SalaryTo,
                m.Format.ToString(),
                m.Tags.Select(t => t.Name).ToArray()))
            .ToListAsync(ct);

        var markers = new List<MapMarkerResponse>(jobs.Count + events.Count + mentorships.Count);
        markers.AddRange(jobs);
        markers.AddRange(events);
        markers.AddRange(mentorships);

        return Ok(markers);
    }
}
