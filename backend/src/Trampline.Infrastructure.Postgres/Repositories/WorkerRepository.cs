using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models.Worker;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class WorkerRepository(ILogger<WorkerRepository> logger, AppDbContext context) : IWorkerRepository
{
    public async Task<WorkerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.WorkerProfiles
            .Include(x => x.JobApplications)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<WorkerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.WorkerProfiles
            .Include(x => x.JobApplications)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(WorkerProfile profile, CancellationToken cancellationToken)
    {
        await context.WorkerProfiles.AddAsync(profile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WorkerProfile profile, CancellationToken cancellationToken)
    {
        context.WorkerProfiles.Update(profile);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var profile = await context.WorkerProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (profile != null)
        {
            context.WorkerProfiles.Remove(profile);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        return await context.WorkerProfiles.CountAsync(cancellationToken);
    }

    public async Task<IDictionary<Guid, WorkerProfile>> GetByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.ToList();
        return await context.WorkerProfiles
            .AsNoTracking()
            .Where(w => ids.Contains(w.UserId))
            .ToDictionaryAsync(w => w.UserId, cancellationToken);
    }

    public async Task<(IEnumerable<WorkerProfile>, int)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? skills = null,
        string? university = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.WorkerProfiles
            .Include(x => x.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Name.ToLower().Contains(search.ToLower()) ||
                x.LastName.ToLower().Contains(search.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(skills))
        {
            var skillList = skills.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (skillList.Length > 0)
            {
                query = query.Where(x => x.Skills.Any(s => skillList.Contains(s)));
            }
        }

        if (!string.IsNullOrWhiteSpace(university))
        {
            query = query.Where(x => x.Info != null &&
                x.Info.University.ToLower().Contains(university.ToLower()));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}