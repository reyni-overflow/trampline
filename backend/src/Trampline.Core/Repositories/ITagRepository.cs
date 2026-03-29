using Trampline.Core.Models.Employee;

namespace Trampline.Core.Repositories;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllAsync(CancellationToken cancellationToken);
    Task<IEnumerable<Tag>> GetAllWithUsageAsync(CancellationToken cancellationToken);
    Task<IEnumerable<(Tag Tag, int JobCount, int EventCount)>> GetAllWithStatsAsync(CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken);
    Task AddAsync(Tag tag, CancellationToken cancellationToken);
    Task DeleteByNameAsync(string name, CancellationToken cancellationToken);
}
