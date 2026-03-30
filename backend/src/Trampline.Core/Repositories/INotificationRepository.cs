namespace Trampline.Core.Repositories;

public interface INotificationRepository
{
    Task<(IEnumerable<object> Items, int Total)> GetPaginatedAsync(Guid userId, int pageNumber, int pageSize, CancellationToken ct);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct);
    Task<bool> MarkAsReadAsync(Guid id, Guid userId, CancellationToken ct);
    Task MarkAllAsReadAsync(Guid userId, CancellationToken ct);
}
