using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Infrastructure.Postgres.Repositories;

public class AuditLogRepository(ILogger<AuditLogRepository> logger, AppDbContext context) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
    {
        await context.AuditLogs.AddAsync(log, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task<(List<AuditLog> Items, int Total)> GetPaginatedAsync(int page, int pageSize, string? action, string? entityType, Guid? userId, CancellationToken ct = default)
    {
        var query = context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(action))
            query = query.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
