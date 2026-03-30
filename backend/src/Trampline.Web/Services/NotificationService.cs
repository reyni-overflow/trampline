using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Trampline.Application.Services;
using Trampline.Core.Models;
using Trampline.Infrastructure.Postgres.Data;
using Trampline.Web.Hubs;
using Lib.Net.Http.WebPush;
using Lib.Net.Http.WebPush.Authentication;

namespace Trampline.Web.Services;

public class NotificationService(
    IHubContext<NotificationHub> hubContext,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration) : INotificationService
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
        _ = SendWebPushAsync(userId, title, message, link, ct);
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

    private async Task SendWebPushAsync(Guid userId, string title, string message, string? link, CancellationToken ct)
    {
        try
        {
            var publicKey = configuration["Vapid:PublicKey"];
            var privateKey = configuration["Vapid:PrivateKey"];
            var subject = configuration["Vapid:Subject"] ?? "mailto:noreply@trampline.ru";

            if (string.IsNullOrEmpty(publicKey) || string.IsNullOrEmpty(privateKey))
                return;

            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var subscriptions = await db.PushSubscriptions
                .Where(s => s.UserId == userId)
                .ToListAsync(ct);

            if (subscriptions.Count == 0) return;

            var client = new PushServiceClient();
            client.DefaultAuthentication = new VapidAuthentication(publicKey, privateKey) { Subject = subject };

            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                title,
                body = message,
                url = link,
                icon = "/favicon.svg"
            });

            foreach (var sub in subscriptions)
            {
                try
                {
                    var pushSub = new Lib.Net.Http.WebPush.PushSubscription { Endpoint = sub.Endpoint };
                    pushSub.SetKey(PushEncryptionKeyName.P256DH, sub.P256dh);
                    pushSub.SetKey(PushEncryptionKeyName.Auth, sub.Auth);
                    var pushMsg = new PushMessage(payload) { Topic = "notification", Urgency = PushMessageUrgency.Normal };
                    await client.RequestPushMessageDeliveryAsync(pushSub, pushMsg, ct);
                }
                catch (PushServiceClientException)
                {
                    db.PushSubscriptions.Remove(sub);
                }
            }

            await db.SaveChangesAsync(ct);
        }
        catch
        {
            // web push failure should not break other notifications
        }
    }

    private static string JSON(object payload)
    {
        return System.Text.Json.JsonSerializer.Serialize(payload);
    }
}
