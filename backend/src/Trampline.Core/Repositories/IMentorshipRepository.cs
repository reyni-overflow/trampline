using Trampline.Core.Models.Employee;

namespace Trampline.Core.Repositories;

public interface IMentorshipRepository
{
    Task<(IEnumerable<Mentorship>, int)> GetPaginationAsync(int pageNumber,
        int pageSize,
        string? city = null,
        int? salaryMin = null,
        int? salaryMax = null,
        string? search = null,
        string? format = null,
        string? tags = null,
        CancellationToken cancellationToken = default);

    Task<Mentorship?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Mentorship?> GetByEmployeeAsync(Guid id, Guid employeeId, CancellationToken cancellationToken);

    Task<Mentorship?> GetByTitleAsync(string title, CancellationToken cancellationToken);

    Task<Mentorship?> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<IEnumerable<Mentorship>> GetAllByUserIdAsync(Guid userId, int pageNumber, int pageSize,
        CancellationToken cancellationToken);

    Task AddAsync(Mentorship profile, CancellationToken cancellationToken);

    Task UpdateAsync(Mentorship profile, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<Mentorship>> GetAllAsync(CancellationToken cancellationToken);

    Task<IEnumerable<Tag>> GetOrCreateTagsAsync(IEnumerable<Tag> tags, CancellationToken cancellationToken);

    Task<IEnumerable<Mentorship>> GetPendingModerationAsync(CancellationToken cancellationToken);

    Task<IDictionary<Guid, Mentorship>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);

    Task SoftDeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
