using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class FavoriteRepository(ILogger<FavoriteRepository> logger, AppDbContext context) : IFavoriteRepository
{
    public async Task<IEnumerable<Favorite>> GetByUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Favorite?> FindAsync(Guid userId, Guid targetId, FavoriteType type, CancellationToken cancellationToken)
    {
        return await context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.TargetId == targetId && f.Type == type, cancellationToken);
    }

    public async Task<Favorite> AddAsync(Favorite favorite, CancellationToken cancellationToken)
    {
        await context.Favorites.AddAsync(favorite, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return favorite;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var fav = await context.Favorites.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        if (fav != null)
        {
            context.Favorites.Remove(fav);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<HashSet<Guid>> GetFavoriteTargetIdsAsync(Guid userId, FavoriteType type, IEnumerable<Guid> targetIds, CancellationToken cancellationToken)
    {
        var ids = targetIds.ToList();
        return await context.Favorites
            .Where(f => f.UserId == userId && f.Type == type && ids.Contains(f.TargetId))
            .Select(f => f.TargetId)
            .ToHashSetAsync(cancellationToken);
    }

    public async Task<bool> IsFavoriteAsync(Guid userId, Guid targetId, FavoriteType type, CancellationToken cancellationToken)
    {
        return await context.Favorites
            .AnyAsync(f => f.UserId == userId && f.TargetId == targetId && f.Type == type, cancellationToken);
    }
}
