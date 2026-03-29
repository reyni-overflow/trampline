using Trampline.Core.Models.Employee;

namespace Trampline.Core.Repositories;

public interface IEventRepository
{
    Task<(IEnumerable<Event>, int)> GetPaginationAsync(int pageNumber,
        int pageSize,
        string? city = null,
        int? salaryMin = null,
        int? salaryMax = null,
        string? search = null,
        string? format = null,
        string? tags = null,
        CancellationToken cancellationToken = default);

    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Event?> GetByEmployeeAsync(Guid id, Guid employeeId, CancellationToken cancellationToken);

    Task<Event?> GetByTitleAsync(string title, CancellationToken cancellationToken);

    Task<Event> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<IEnumerable<Event>> GetAllByUserIdAsync(Guid userId, int pageNumber, int pageSize,
        CancellationToken cancellationToken);

    Task AddAsync(Event profile, CancellationToken cancellationToken);

    Task UpdateAsync(Event profile, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken);

    Task<IEnumerable<Tag>> GetOrCreateTagsAsync(IEnumerable<Tag> tags, CancellationToken cancellationToken);

    Task<IEnumerable<Event>> GetPendingModerationAsync(CancellationToken cancellationToken);

    Task<IDictionary<Guid, Event>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}