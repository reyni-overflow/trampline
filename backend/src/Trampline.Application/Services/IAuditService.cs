namespace Trampline.Application.Services;

public interface IAuditService
{
    Task LogAsync(Guid? userId, string userName, string userRole, string action, string entityType, Guid? entityId, string? details = null, string? ipAddress = null, CancellationToken ct = default);
}
