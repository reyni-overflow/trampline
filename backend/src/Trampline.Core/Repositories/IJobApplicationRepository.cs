using Trampline.Core.Models.Employee;

namespace Trampline.Core.Repositories;

public interface IJobApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<JobApplication>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<JobApplication> AddAsync(JobApplication jobApplication, CancellationToken cancellationToken = default);

    Task UpdateAsync(JobApplication jobApplication, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}