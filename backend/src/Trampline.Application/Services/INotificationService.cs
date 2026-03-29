namespace Trampline.Application.Services;

public interface INotificationService
{
    Task SendAsync(Guid userId, string type, object payload, CancellationToken ct = default);
    Task SendAsync(IEnumerable<Guid> userIds, string type, object payload, CancellationToken ct = default);
    Task SendAsync(Guid userId, string type, string title, string message, object payload, string? link = null, CancellationToken ct = default);
    Task SendAsync(IEnumerable<Guid> userIds, string type, string title, string message, object payload, string? link = null, CancellationToken ct = default);
}
