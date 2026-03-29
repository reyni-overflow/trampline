using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class UserRepository(AppDbContext context, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting user with id {userId}", userId);

        return await context.Users
            .Include(x => x.Sessions)
            .Include(x => x.EmployeeProfile)
            .ThenInclude(x => x!.Jobs)
            .Include(x => x.WorkerProfile)
            .ThenInclude(x => x!.JobApplications)
            .ThenInclude(x => x.Job)
            .ThenInclude(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }

    public async Task<T?> GetMeAsync<T>(Guid id, Expression<Func<User, T>> expression, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(expression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting user with email {email}", email);

        return await context.Users
            .Include(x => x.Sessions)
            .Include(x => x.EmployeeProfile)
            .ThenInclude(x => x!.Jobs)
            .Include(x => x.WorkerProfile)
            .ThenInclude(x => x!.JobApplications)
            .ThenInclude(x => x.Job)
            .ThenInclude(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<User?> GetByPhoneAsync(string phone, CancellationToken cancellationToken)
    {
        return await context.Users
            .Include(x => x.Sessions)
            .Include(x => x.EmployeeProfile)
            .ThenInclude(x => x!.Jobs)
            .Include(x => x.WorkerProfile)
            .ThenInclude(x => x!.JobApplications)
            .ThenInclude(x => x.Job)
            .ThenInclude(x => x.Employee)
            .FirstOrDefaultAsync(x => x.Phone == phone, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        logger.LogDebug("Adding user with id {userId}", user.Id);
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        logger.LogDebug("Updating user with id {userId}", user.Id);
        context.Users.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Deleting user with id {userId}", userId);
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Getting all users");
        return await context.Users
            .Include(x => x.Sessions)
            .Include(x => x.EmployeeProfile)
            .Include(x => x.WorkerProfile)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<User> Items, int Total)> GetPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Users
            .Include(x => x.Sessions)
            .Include(x => x.EmployeeProfile)
            .Include(x => x.WorkerProfile);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.Sessions.Min(s => (DateTime?)s.CreatedAt))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(Role role, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);
    }

    public async Task<IDictionary<Guid, User>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        return await context.Users
            .AsNoTracking()
            .Where(u => idList.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);
    }
}