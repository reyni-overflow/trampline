using System.Linq.Expressions;
using Trampline.Core.Models;

namespace Trampline.Core.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<T?> GetMeAsync<T>(Guid id, Expression<Func<User, T>> expression, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken);

    Task AddAsync(User user, CancellationToken cancellationToken);

    Task UpdateAsync(User user, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken);

    Task<(IEnumerable<User> Items, int Total)> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<IEnumerable<User>> GetByRoleAsync(Role role, CancellationToken cancellationToken);

    Task<IDictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
}