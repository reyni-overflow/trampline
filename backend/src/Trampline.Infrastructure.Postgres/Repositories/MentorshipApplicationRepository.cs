using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class MentorshipApplicationRepository(ILogger<MentorshipApplicationRepository> logger, AppDbContext context) : IMentorshipApplicationRepository
{
    public async Task<MentorshipApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.MentorshipApplications
            .Include(x => x.Profile)
            .Include(x => x.Mentorship)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<MentorshipApplication>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.MentorshipApplications.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<MentorshipApplication>> GetByWorkerProfileIdAsync(Guid workerProfileId, CancellationToken ct = default)
    {
        return await context.MentorshipApplications
            .Include(x => x.Profile)
            .Include(x => x.Mentorship)
            .ThenInclude(x => x.Profile)
            .Where(x => x.WorkerProfileId == workerProfileId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<MentorshipApplication> AddAsync(MentorshipApplication mentorshipApplication, CancellationToken cancellationToken = default)
    {
        await context.MentorshipApplications.AddAsync(mentorshipApplication, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return mentorshipApplication;
    }

    public async Task UpdateAsync(MentorshipApplication mentorshipApplication, CancellationToken cancellationToken = default)
    {
        context.MentorshipApplications.Update(mentorshipApplication);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var mentorshipApplication = await GetByIdAsync(id, cancellationToken);
        if (mentorshipApplication != null)
        {
            context.MentorshipApplications.Remove(mentorshipApplication);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
