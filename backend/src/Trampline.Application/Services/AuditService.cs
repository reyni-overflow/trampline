using Microsoft.Extensions.Logging;
using Trampline.Core.Models;
using Trampline.Core.Repositories;

namespace Trampline.Application.Services;

public class AuditService(
    IAuditLogRepository auditLogRepository,
    ILogger<AuditService> logger) : IAuditService
{
    public async Task LogAsync(Guid? userId, string userName, string userRole, string action, string entityType, Guid? entityId, string? details = null, string? ipAddress = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = userName,
            UserRole = userRole,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        await auditLogRepository.AddAsync(log, ct);
        logger.LogInformation("Audit: {Action} on {EntityType} by {UserName}", action, entityType, userName);
    }
}
