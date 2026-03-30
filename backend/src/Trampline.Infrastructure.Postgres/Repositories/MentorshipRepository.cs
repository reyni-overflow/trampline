using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class MentorshipRepository(ILogger<MentorshipRepository> logger, AppDbContext context) : IMentorshipRepository
{
    public async Task<(IEnumerable<Mentorship>, int)> GetPaginationAsync(int pageNumber,
        int pageSize,
        string? city = null,
        int? salaryMin = null,
        int? salaryMax = null,
        string? search = null,
        string? format = null,
        string? tags = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Mentorships
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
            var s = search.Trim();
            if (s.Length <= 2)
            {
                var sl = s.ToLower();
                query = query.Where(x => x.Title.ToLower().Contains(sl));
            }
            else
            {
                query = query.Where(x =>
                    EF.Functions.ToTsVector("russian", x.Title + " " + x.Description)
                        .Matches(EF.Functions.WebSearchToTsQuery("russian", s)) ||
                    EF.Functions.ToTsVector("simple", x.Title + " " + x.Description)
                        .Matches(EF.Functions.WebSearchToTsQuery("simple", s)));
            }
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

    public async Task<Mentorship?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Mentorships
            .Include(x => x.Profile)
            .Include(x => x.MentorshipApplications)
            .ThenInclude(x => x.Profile)
            .Include(x => x.Tags)
            .Where(e => e.DeletedAt == null)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Mentorship?> GetByEmployeeAsync(Guid id, Guid employeeId, CancellationToken cancellationToken)
    {
        return await context.Mentorships
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id && x.EmployeeId == employeeId, cancellationToken);
    }

    public async Task<Mentorship?> GetByTitleAsync(string title, CancellationToken cancellationToken)
    {
        return await context.Mentorships
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .Where(e => e.DeletedAt == null)
            .FirstOrDefaultAsync(e => e.Title == title, cancellationToken);
    }

    public async Task<Mentorship> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return (await context.Mentorships
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken))!;
    }

    public async Task<IEnumerable<Mentorship>> GetAllByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await context.Mentorships
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .Where(x => x.UserId == userId)
            .Where(x => x.IsActive)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Mentorship profile, CancellationToken cancellationToken)
    {
        await context.Mentorships.AddAsync(profile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Mentorship profile, CancellationToken cancellationToken)
    {
        context.Mentorships.Update(profile);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var find = await context.Mentorships.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (find != null)
        {
            context.Mentorships.Remove(find);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Mentorship>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Mentorships
            .Include(x => x.Profile)
            .Include(x => x.Tags)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Mentorship>> GetPendingModerationAsync(CancellationToken cancellationToken)
    {
        return await context.Mentorships
            .AsNoTracking()
            .Include(e => e.Tags)
            .Where(e => !e.IsActive)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IDictionary<Guid, Mentorship>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        return await context.Mentorships
            .AsNoTracking()
            .Include(m => m.Profile)
            .Include(m => m.Tags)
            .Where(m => idList.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, cancellationToken);
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
