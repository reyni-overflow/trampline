using Trampline.Core.Models;

namespace Trampline.Core.Repositories;

public interface IFavoriteRepository
{
    Task<IEnumerable<Favorite>> GetByUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<Favorite?> FindAsync(Guid userId, Guid targetId, FavoriteType type, CancellationToken cancellationToken);

    Task<Favorite> AddAsync(Favorite favorite, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task<HashSet<Guid>> GetFavoriteTargetIdsAsync(Guid userId, FavoriteType type, IEnumerable<Guid> targetIds, CancellationToken cancellationToken);

    Task<bool> IsFavoriteAsync(Guid userId, Guid targetId, FavoriteType type, CancellationToken cancellationToken);
}
