using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class ReviewRepository(ILogger<ReviewRepository> logger, AppDbContext context) : IReviewRepository
{
    public async Task<List<Review>> GetApprovedAsync(CancellationToken ct = default)
    {
        return await context.Reviews
            .Where(r => r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Review>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.Reviews
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Review> AddAsync(Review review, CancellationToken ct = default)
    {
        await context.Reviews.AddAsync(review, ct);
        await context.SaveChangesAsync(ct);
        return review;
    }

    public async Task<Review?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (review != null)
        {
            context.Reviews.Remove(review);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<Review> UpdateAsync(Review review, CancellationToken ct = default)
    {
        context.Reviews.Update(review);
        await context.SaveChangesAsync(ct);
        return review;
    }
}
