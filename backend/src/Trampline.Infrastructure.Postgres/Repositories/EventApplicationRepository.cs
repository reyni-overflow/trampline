using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class EventApplicationRepository(ILogger<EventApplicationRepository> logger, AppDbContext context) : IEventApplicationRepository
{
    public async Task<EventApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.EventApplications
            .Include(x => x.Profile)
            .Include(x => x.Event)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<EventApplication>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.EventApplications.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EventApplication>> GetByWorkerProfileIdAsync(Guid workerProfileId, CancellationToken ct = default)
    {
        return await context.EventApplications
            .Include(x => x.Profile)
            .Include(x => x.Event)
            .ThenInclude(x => x.Profile)
            .Where(x => x.WorkerProfileId == workerProfileId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<EventApplication> AddAsync(EventApplication eventApplication, CancellationToken cancellationToken = default)
    {
        await context.EventApplications.AddAsync(eventApplication, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return eventApplication;
    }

    public async Task UpdateAsync(EventApplication eventApplication, CancellationToken cancellationToken = default)
    {
        context.EventApplications.Update(eventApplication);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventApplication = await GetByIdAsync(id, cancellationToken);
        if (eventApplication != null)
        {
            context.EventApplications.Remove(eventApplication);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}