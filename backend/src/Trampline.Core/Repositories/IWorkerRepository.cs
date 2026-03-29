using Trampline.Core.Models.Worker;

namespace Trampline.Core.Repositories;

public interface IWorkerRepository
{
    Task<WorkerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<WorkerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(WorkerProfile profile, CancellationToken cancellationToken);

    Task UpdateAsync(WorkerProfile profile, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<int> CountAsync(CancellationToken cancellationToken);

    Task<(IEnumerable<WorkerProfile>, int)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? skills = null,
        string? university = null,
        CancellationToken cancellationToken = default);

    Task<IDictionary<Guid, WorkerProfile>> GetByUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken);
}