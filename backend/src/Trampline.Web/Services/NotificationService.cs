using Microsoft.AspNetCore.SignalR;
using Trampline.Application.Services;
using Trampline.Core.Models;
using Trampline.Infrastructure.Postgres.Data;
using Trampline.Web.Hubs;

namespace Trampline.Web.Services;

public class NotificationService(
    IHubContext<NotificationHub> hubContext,
    IServiceScopeFactory scopeFactory) : INotificationService
{
    public async Task SendAsync(Guid userId, string type, object payload, CancellationToken ct)
    {
        await PersistAsync(userId, type, type, JSON(payload), null, ct);
        await hubContext.Clients.Group(userId.ToString()).SendAsync("Notification", new { type, payload }, ct);
    }

    public async Task SendAsync(IEnumerable<Guid> userIds, string type, object payload, CancellationToken ct)
    {
        var tasks = userIds.Select(id => SendAsync(id, type, payload, ct));
        await Task.WhenAll(tasks);
    }

    public async Task SendAsync(Guid userId, string type, string title, string message, object payload, string? link = null, CancellationToken ct = default)
    {
        await PersistAsync(userId, type, title, message, link, ct);
        await hubContext.Clients.Group(userId.ToString()).SendAsync("Notification", new { type, payload }, ct);
    }

    public async Task SendAsync(IEnumerable<Guid> userIds, string type, string title, string message, object payload, string? link = null, CancellationToken ct = default)
    {
        var tasks = userIds.Select(id => SendAsync(id, type, title, message, payload, link, ct));
        await Task.WhenAll(tasks);
    }

    private async Task PersistAsync(Guid userId, string type, string title, string message, string? link, CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var notification = Notification.Create(userId, type, title, message, link);
            db.Notifications.Add(notification);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // persistence failure should not break real-time delivery
        }
    }

    private static string JSON(object payload)
    {
        return System.Text.Json.JsonSerializer.Serialize(payload);
    }
}
