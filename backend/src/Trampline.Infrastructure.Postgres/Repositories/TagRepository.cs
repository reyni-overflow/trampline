using Microsoft.EntityFrameworkCore;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class TagRepository(AppDbContext context) : ITagRepository
{
    public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Tags
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Tag>> GetAllWithUsageAsync(CancellationToken cancellationToken)
    {
        return await context.Tags
            .AsNoTracking()
            .Include(t => t.Jobs)
            .OrderByDescending(t => t.Jobs!.Count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<(Tag Tag, int JobCount, int EventCount)>> GetAllWithStatsAsync(CancellationToken cancellationToken)
    {
        var stats = await context.Tags
            .AsNoTracking()
            .Select(t => new
            {
                Tag = t,
                JobCount = t.Jobs.Count,
                EventCount = t.Events.Count
            })
            .OrderByDescending(t => t.JobCount + t.EventCount)
            .ToListAsync(cancellationToken);

        return stats.Select(s => (s.Tag, s.JobCount, s.EventCount));
    }

    public async Task<bool> ExistsAsync(string name, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim().ToLower();
        return await context.Tags.AnyAsync(t => t.Name.ToLower() == trimmed, cancellationToken);
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken)
    {
        await context.Tags.AddAsync(tag, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByNameAsync(string name, CancellationToken cancellationToken)
    {
        var tag = await context.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        if (tag != null)
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
