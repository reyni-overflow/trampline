using Trampline.Core.Models.Employee;

namespace Trampline.Core.Repositories;

public interface IEmployeeRepository
{
    Task<EmployeeProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(IEnumerable<EmployeeProfile>, int)> GetPaginationAsync(int pageNumber, int pageSize, string? search, string? activity, CancellationToken cancellationToken);

    Task<EmployeeProfile?> GetByUserIdAsync(Guid id, CancellationToken cancellationToken);

    Task AddAsync(EmployeeProfile profile, CancellationToken cancellationToken);

    Task UpdateAsync(EmployeeProfile profile, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<EmployeeProfile>> GetUnverifiedAsync(CancellationToken cancellationToken);

    Task<IDictionary<Guid, EmployeeProfile>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}