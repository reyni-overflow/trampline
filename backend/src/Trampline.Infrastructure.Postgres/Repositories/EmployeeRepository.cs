using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class EmployeeRepository(ILogger<EmployeeRepository> logger, AppDbContext context) : IEmployeeRepository
{
    public async Task<EmployeeProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.EmployeeProfiles
            .Include(x => x.Jobs)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<(IEnumerable<EmployeeProfile>, int)> GetPaginationAsync(int pageNumber,
        int pageSize, string? search, string? activity,
        CancellationToken cancellationToken)
    {
        var query = context.EmployeeProfiles
            .Include(x => x.Jobs)
            .Include(x => x.User)
            .Where(x => x.VerificationLevel >= 1)
            .Where(x => !x.User.IsPrivate);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(q) || x.Activity.ToLower().Contains(q));
        }

        if (!string.IsNullOrWhiteSpace(activity))
            query = query.Where(x => x.Activity.ToLower().Contains(activity.Trim().ToLower()));

        var total = await query.CountAsync(cancellationToken);

        var list = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (list, total);
    }

    public async Task<EmployeeProfile?> GetByUserIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.EmployeeProfiles
            .Include(x => x.Jobs)
            .FirstOrDefaultAsync(x => x.UserId == id, cancellationToken);
    }

    public async Task AddAsync(EmployeeProfile profile, CancellationToken cancellationToken)
    {
        await context.EmployeeProfiles.AddAsync(profile, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmployeeProfile profile, CancellationToken cancellationToken)
    {
        context.EmployeeProfiles.Update(profile);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var profile = await context.EmployeeProfiles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (profile != null)
        {
            context.EmployeeProfiles.Remove(profile);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<EmployeeProfile>> GetUnverifiedAsync(CancellationToken cancellationToken)
    {
        return await context.EmployeeProfiles
            .AsNoTracking()
            .Include(x => x.User)
            .ThenInclude(x => x.Sessions)
            .Where(e => e.VerificationLevel == 1)
            .ToListAsync(cancellationToken);
    }

    public async Task<IDictionary<Guid, EmployeeProfile>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        return await context.EmployeeProfiles
            .AsNoTracking()
            .Include(x => x.Jobs)
            .Where(x => idList.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
    }
}