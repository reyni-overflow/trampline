using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class UserSessionRepository(ILogger<UserSessionRepository> logger, AppDbContext context) : IUserSessionRepository
{
    public async Task<UserSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var fnid = await context.Sessions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return fnid;
    }

    public async Task<UserSession?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken)
    {
        var fnid = await context.Sessions.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        return fnid;
    }

    public async Task<List<UserSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var fnid = context.Sessions.Where(x => x.UserId == userId);

        return await fnid.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserSession session, CancellationToken cancellationToken)
    {
        await context.Sessions.AddAsync(session, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(UserSession session, CancellationToken cancellationToken)
    {
        context.Sessions.Update(session);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var find = await GetByIdAsync(id, cancellationToken);

        if (find != null)
        {
            context.Sessions.Remove(find);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}