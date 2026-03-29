using Trampline.Core.Models.Worker;

namespace Trampline.Core.Repositories;

public interface IRecommendationRepository
{
    Task AddAsync(Recommendation recommendation, CancellationToken cancellationToken);
    Task<IEnumerable<Recommendation>> GetByReceiverAsync(Guid userId, CancellationToken cancellationToken);
}
