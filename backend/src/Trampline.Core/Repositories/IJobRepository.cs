using Trampline.Core.Models.Employee;

namespace Trampline.Core.Repositories;

public interface IJobRepository
{
    Task<(IEnumerable<Job>, int)> GetPaginationAsync(int pageNumber,
        int pageSize,
        string? city = null,
        int? salaryMin = null,
        int? salaryMax = null,
        string? search = null,
        string? type = null,
        string? format = null,
        string? tags = null,
        CancellationToken cancellationToken = default);

    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Job?> GetByEmployeeAsync(Guid id, Guid employeeId, CancellationToken cancellationToken);

    Task<Job?> GetByTitleAsync(string title, CancellationToken cancellationToken);

    Task<Job?> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<IEnumerable<Job>> GetAllByUserIdAsync(Guid userId, int pageNumber, int pageSize,
        CancellationToken cancellationToken);

    Task AddAsync(Job profile, CancellationToken cancellationToken);

    Task UpdateAsync(Job profile, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken);

    Task<IEnumerable<Tag>> GetOrCreateTagsAsync(IEnumerable<Tag> tags, CancellationToken cancellationToken);

    Task<IEnumerable<Job>> GetPendingModerationAsync(CancellationToken cancellationToken);

    Task<IDictionary<Guid, Job>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    Task<object> GetResponseStatsAsync(Guid userId, CancellationToken cancellationToken);

    Task SoftDeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}