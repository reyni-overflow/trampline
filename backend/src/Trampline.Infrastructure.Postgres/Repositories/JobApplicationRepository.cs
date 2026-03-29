using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class JobApplicationRepository(ILogger<JobApplicationRepository> logger, AppDbContext context) : IJobApplicationRepository
{
    public async Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.JobApplications
            .Include(x => x.Profile)
            .Include(x => x.Job)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<JobApplication>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.JobApplications.ToListAsync(cancellationToken);
    }

    public async Task<JobApplication> AddAsync(JobApplication jobApplication, CancellationToken cancellationToken = default)
    {
        await context.JobApplications.AddAsync(jobApplication, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return jobApplication;
    }

    public async Task UpdateAsync(JobApplication jobApplication, CancellationToken cancellationToken = default)
    {
        context.JobApplications.Update(jobApplication);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var jobApplication = await GetByIdAsync(id, cancellationToken);
        if (jobApplication != null)
        {
            context.JobApplications.Remove(jobApplication);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}