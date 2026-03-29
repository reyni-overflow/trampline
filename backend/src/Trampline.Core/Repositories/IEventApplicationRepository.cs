using Trampline.Core.Models.Employee;

namespace Trampline.Core.Repositories;

public interface IEventApplicationRepository
{
    Task<EventApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<EventApplication>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<EventApplication>> GetByWorkerProfileIdAsync(Guid workerProfileId, CancellationToken ct = default);

    Task<EventApplication> AddAsync(EventApplication eventApplication, CancellationToken cancellationToken = default);

    Task UpdateAsync(EventApplication eventApplication, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
