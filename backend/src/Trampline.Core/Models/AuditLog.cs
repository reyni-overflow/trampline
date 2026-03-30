namespace Trampline.Core.Models;

public class AuditLog
{
    public Guid Id { get; private set; }

    public Guid? UserId { get; private set; }

    public string UserName { get; private set; } = "";

    public string UserRole { get; private set; } = "";

    public string Action { get; private set; } = "";

    public string EntityType { get; private set; } = "";

    public Guid? EntityId { get; private set; }

    public string? Details { get; private set; }

    public string? IpAddress { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private AuditLog() { }

    public static AuditLog Create(Guid? userId, string userName, string userRole, string action, string entityType, Guid? entityId, string? details = null, string? ipAddress = null)
    {
        return new AuditLog
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
    }
}
