using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class JobRepository(ILogger<JobRepository> logger, AppDbContext context) : IJobRepository
{
    public async Task<(IEnumerable<Job>, int)> GetPaginationAsync(int pageNumber,
        int pageSize,
        string? city = null,
        int? salaryMin = null,
        int? salaryMax = null,
        string? search = null,
        string? type = null,
        string? format = null,
        string? tags = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Jobs
            .Include(x => x.Employee)
            .Include(x => x.Tags)
            .Where(x => x.IsActive)
            .Where(x => x.Employee.VerificationLevel >= 1);

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

        if (!string.IsNullOrWhiteSpace(type))
        {
            var typeValues = type.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => Enum.TryParse<JobType>(t, true, out var jt) ? jt : (JobType?)null)
                .Where(t => t.HasValue)
                .Select(t => t!.Value)
                .ToList();
            if (typeValues.Count > 0)
                query = query.Where(x => typeValues.Contains(x.Type));
        }

        if (!string.IsNullOrWhiteSpace(format))
        {
            var formatValues = format.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(f => Enum.TryParse<WorkFormat>(f, true, out var wf) ? wf : (WorkFormat?)null)
                .Where(f => f.HasValue)
                .Select(f => f!.Value)
                .ToList();
            if (formatValues.Count > 0)
                query = query.Where(x => formatValues.Contains(x.Format));
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

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Jobs
            .Include(x => x.JobApplications)
            .ThenInclude(x => x.Profile)
            .Include(x => x.Tags)
            .Where(j => j.DeletedAt == null)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<Job?> GetByEmployeeAsync(Guid id, Guid employeeId, CancellationToken cancellationToken)
    {
        return await context.Jobs
            .Include(x => x.JobApplications)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == employeeId, cancellationToken);
    }

    public async Task<Job> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return (await context.Jobs
            .Include(x => x.JobApplications)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken))!;
    }

    public async Task<IEnumerable<Job>> GetAllByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        return await context.Jobs
            .Include(x => x.JobApplications)
            .Include(x => x.Tags)
            .Where(x => x.UserId == userId)
            .Where(x => x.IsActive)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Job?> GetByTitleAsync(string title, CancellationToken cancellationToken)
    {
        return await context.Jobs
            .Include(x => x.JobApplications)
            .Include(x => x.Tags)
            .Where(j => j.DeletedAt == null)
            .FirstOrDefaultAsync(j => j.Title == title, cancellationToken);
    }

    public async Task AddAsync(Job profile, CancellationToken cancellationToken)
    {
        await context.Jobs.AddAsync(profile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Job profile, CancellationToken cancellationToken)
    {
        context.Jobs.Update(profile);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var find = await context.Jobs.FirstOrDefaultAsync(j => j.Id == id, cancellationToken);

        if (find != null)
        {
            context.Jobs.Remove(find);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Jobs
            .Include(x => x.JobApplications)
            .Include(x => x.Tags)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetPendingModerationAsync(CancellationToken cancellationToken)
    {
        return await context.Jobs
            .AsNoTracking()
            .Include(j => j.Tags)
            .Where(j => !j.IsActive)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IDictionary<Guid, Job>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        return await context.Jobs
            .AsNoTracking()
            .Include(j => j.Employee)
            .Where(j => idList.Contains(j.Id))
            .ToDictionaryAsync(j => j.Id, cancellationToken);
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