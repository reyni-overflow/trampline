using Trampline.Core.Models;

namespace Trampline.Core.Repositories;

public interface IReviewRepository
{
    Task<List<Review>> GetApprovedAsync(CancellationToken ct = default);

    Task<List<Review>> GetAllAsync(CancellationToken ct = default);

    Task<Review> AddAsync(Review review, CancellationToken ct = default);

    Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task<Review> UpdateAsync(Review review, CancellationToken ct = default);
}
