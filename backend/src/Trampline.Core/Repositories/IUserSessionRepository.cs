using Trampline.Core.Models;

namespace Trampline.Core.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<UserSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<List<UserSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(UserSession session, CancellationToken cancellationToken);

    Task UpdateAsync(UserSession session, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}