using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class EventRepository(ILogger<EventRepository> logger, AppDbContext context) : IEventRepository
{
    public async Task<(IEnumerable<Event>, int)> GetPaginationAsync(int pageNumber,
        int pageSize,
        string? city = null,
        int? salaryMin = null,
        int? salaryMax = null,
        string? search = null,
        string? format = null,
        string? tags = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Events
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .Where(x => x.IsActive)
            .Where(x => x.Profile.VerificationLevel >= 1);

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(x => x.City != null && x.City.ToLower().Contains(city.ToLower()));
        }

        if (salaryMin.HasValue)
        {
            query = query.Where(x => x.SalaryFrom >= salaryMin.Value);
        }

        if (salaryMax.HasValue)
        {
            query = query.Where(x => x.SalaryTo <= salaryMax.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(x =>
                (x.Title != null && x.Title.ToLower().Contains(s)) ||
                (x.Description != null && x.Description.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(format) && Enum.TryParse<WorkFormat>(format, true, out var workFormat))
        {
            query = query.Where(x => x.Format == workFormat);
        }

        if (!string.IsNullOrWhiteSpace(tags))
        {
            var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tagList.Length > 0)
            {
                query = query.Where(x => x.Tags.Any(t => tagList.Contains(t.Name)));
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Events
            .Include(x => x.Profile)
            .Include(x => x.EventApplications)
            .ThenInclude(x => x.Profile)
            .Include(x => x.Tags)
            .Where(e => e.DeletedAt == null)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Event?> GetByEmployeeAsync(Guid id, Guid employeeId, CancellationToken cancellationToken)
    {
        return await context.Events
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmployeeId == employeeId, cancellationToken);
    }

    public async Task<Event?> GetByTitleAsync(string title, CancellationToken cancellationToken)
    {
        return await context.Events
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .Where(e => e.DeletedAt == null)
            .FirstOrDefaultAsync(e => e.Title == title, cancellationToken);
    }

    public async Task<Event> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return (await context.Events
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken))!;
    }

    public async Task<IEnumerable<Event>> GetAllByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await context.Events
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .Where(x => x.UserId == userId)
            .Where(x => x.IsActive)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Event profile, CancellationToken cancellationToken)
    {
        await context.Events.AddAsync(profile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Event profile, CancellationToken cancellationToken)
    {
        context.Events.Update(profile);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var find = await context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (find != null)
        {
            context.Events.Remove(find);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Events
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Event>> GetPendingModerationAsync(CancellationToken cancellationToken)
    {
        return await context.Events
            .AsNoTracking()
            .Include(e => e.Tags)
            .Where(e => !e.IsActive)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IDictionary<Guid, Event>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        return await context.Events
            .AsNoTracking()
            .Include(e => e.Profile)
            .Include(e => e.Tags)
            .Where(e => idList.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetOrCreateTagsAsync(IEnumerable<Tag> tags, CancellationToken cancellationToken)
    {
        var normalizedTags = tags
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .Select(x => new TagRequest
            {
                Name = x.Name.Trim(),
                Category = x.Category.Trim(),
                Lvl = x.Lvl
            })
            .GroupBy(x => new { x.Name, x.Category, x.Lvl })
            .Select(x => x.First())
            .ToList();

        var result = new List<Tag>();

        foreach (var tag in normalizedTags)
        {
            var existingTag = await context.Tags.FirstOrDefaultAsync(x =>
                    x.Name == tag.Name &&
                    x.Category == tag.Category &&
                    x.Lvl == tag.Lvl,
                cancellationToken);

            if (existingTag is not null)
            {
                result.Add(existingTag);
                continue;
            }

            var newTag = new Tag
            {
                Id = Guid.NewGuid(),
                Name = tag.Name,
                Category = tag.Category,
                Lvl = tag.Lvl
            };

            context.Tags.Add(newTag);
            result.Add(newTag);
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            context.ChangeTracker.Clear();

            result.Clear();
            foreach (var tag in normalizedTags)
            {
                var existingTag = await context.Tags.FirstOrDefaultAsync(x =>
                        x.Name == tag.Name &&
                        x.Category == tag.Category &&
                        x.Lvl == tag.Lvl,
                    cancellationToken);

                if (existingTag is not null)
                {
                    result.Add(existingTag);
                }
            }
        }

        return result;
    }
}