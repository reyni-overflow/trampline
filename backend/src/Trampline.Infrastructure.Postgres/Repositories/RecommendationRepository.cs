using Microsoft.EntityFrameworkCore;
using Trampline.Core.Models.Worker;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class RecommendationRepository(AppDbContext context) : IRecommendationRepository
{
    public async Task AddAsync(Recommendation recommendation, CancellationToken cancellationToken)
    {
        await context.Recommendations.AddAsync(recommendation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Recommendation>> GetByReceiverAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Recommendations
            .Where(r => r.ToUserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
