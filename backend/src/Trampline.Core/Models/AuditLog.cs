namespace Trampline.Core.Models;

public class AuditLog
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string UserName { get; set; } = "";

    public string UserRole { get; set; } = "";

    public string Action { get; set; } = "";

    public string EntityType { get; set; } = "";

    public Guid? EntityId { get; set; }

    public string? Details { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
