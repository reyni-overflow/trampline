using Trampline.Core.Models.Employee;

namespace Trampline.Core.Repositories;

public interface IMentorshipApplicationRepository
{
    Task<MentorshipApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<MentorshipApplication>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<MentorshipApplication>> GetByWorkerProfileIdAsync(Guid workerProfileId, CancellationToken ct = default);

    Task<MentorshipApplication> AddAsync(MentorshipApplication mentorshipApplication, CancellationToken cancellationToken = default);

    Task UpdateAsync(MentorshipApplication mentorshipApplication, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
