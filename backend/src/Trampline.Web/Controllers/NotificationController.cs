using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Infrastructure.Postgres.Data;
using Trampline.Web.Controllers.Base;

namespace Trampline.Web.Controllers;

[Authorize]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class NotificationController(
    INotificationRepository notificationRepository,
    AppDbContext dbContext) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetNotificationsAsync(CancellationToken ct,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var userGuid = GetUserId();

        pageSize = Math.Clamp(pageSize, 1, 50);
        pageNumber = Math.Max(pageNumber, 1);

        var (items, total) = await notificationRepository.GetPaginatedAsync(userGuid, pageNumber, pageSize, ct);

        var totalPages = (int)Math.Ceiling((double)total / pageSize);

        return Ok(new
        {
            items,
            totalCount = total,
            totalPages,
            pageSize,
            currentPage = pageNumber,
            hasNextPage = pageNumber < totalPages,
            hasPreviousPage = pageNumber > 1
        });
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCountAsync(CancellationToken ct)
    {
        var userGuid = GetUserId();

        var count = await notificationRepository.GetUnreadCountAsync(userGuid, ct);

        return Ok(new { count });
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsReadAsync(Guid id, CancellationToken ct)
    {
        var userGuid = GetUserId();

        var success = await notificationRepository.MarkAsReadAsync(id, userGuid, ct);
        if (!success) return NotFound();

        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsReadAsync(CancellationToken ct)
    {
        var userGuid = GetUserId();

        await notificationRepository.MarkAllAsReadAsync(userGuid, ct);

        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("vapid-key")]
    public IActionResult GetVapidPublicKey([FromServices] IConfiguration config)
    {
        var key = config["Vapid:PublicKey"];
        if (string.IsNullOrEmpty(key))
            return NotFound();
        return Ok(new { publicKey = key });
    }

    [HttpPost("push-subscribe")]
    public async Task<IActionResult> PushSubscribeAsync(
        [FromBody] PushSubscribeRequest request, CancellationToken ct)
    {
        var userGuid = GetUserId();

        var existing = await dbContext.PushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userGuid && s.Endpoint == request.Endpoint, ct);

        if (existing == null)
        {
            var sub = PushSubscription.Create(userGuid, request.Endpoint, request.P256dh, request.Auth);
            dbContext.PushSubscriptions.Add(sub);
            await dbContext.SaveChangesAsync(ct);
        }

        return Ok();
    }

    [HttpPost("push-unsubscribe")]
    public async Task<IActionResult> PushUnsubscribeAsync(
        [FromBody] PushUnsubscribeRequest request, CancellationToken ct)
    {
        var userGuid = GetUserId();

        await dbContext.PushSubscriptions
            .Where(s => s.UserId == userGuid && s.Endpoint == request.Endpoint)
            .ExecuteDeleteAsync(ct);

        return Ok();
    }
}

public record PushSubscribeRequest(string Endpoint, string P256dh, string Auth);
public record PushUnsubscribeRequest(string Endpoint);
