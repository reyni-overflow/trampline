using Trampline.Core.Models;

namespace Trampline.Core.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);

    Task<(List<AuditLog> Items, int Total)> GetPaginatedAsync(int page, int pageSize, string? action, string? entityType, Guid? userId, CancellationToken ct = default);
}
