using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Core.Models;
using Trampline.Infrastructure.Postgres.Data;

namespace Trampline.Web.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class NotificationController(AppDbContext dbContext) : ControllerBase
{
    private Guid? GetUserId()
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return string.IsNullOrEmpty(val) ? null : new Guid(val);
    }

    [HttpGet]
    public async Task<IActionResult> GetNotificationsAsync(CancellationToken ct,
        int pageNumber = 1,
        int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        pageSize = Math.Clamp(pageSize, 1, 50);
        pageNumber = Math.Max(pageNumber, 1);

        var query = dbContext.Notifications
            .Where(n => n.UserId == userId.Value)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.Type,
                n.Title,
                n.Message,
                n.Link,
                n.IsRead,
                n.CreatedAt
            })
            .ToListAsync(ct);

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
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var count = await dbContext.Notifications
            .CountAsync(n => n.UserId == userId.Value && !n.IsRead, ct);

        return Ok(new { count });
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsReadAsync(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId.Value, ct);
        if (notification == null) return NotFound();

        notification.MarkAsRead();
        await dbContext.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsReadAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        await dbContext.Notifications
            .Where(n => n.UserId == userId.Value && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true), ct);

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
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var existing = await dbContext.PushSubscriptions
            .FirstOrDefaultAsync(s => s.UserId == userId.Value && s.Endpoint == request.Endpoint, ct);

        if (existing == null)
        {
            var sub = PushSubscription.Create(userId.Value, request.Endpoint, request.P256dh, request.Auth);
            dbContext.PushSubscriptions.Add(sub);
            await dbContext.SaveChangesAsync(ct);
        }

        return Ok();
    }

    [HttpPost("push-unsubscribe")]
    public async Task<IActionResult> PushUnsubscribeAsync(
        [FromBody] PushUnsubscribeRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        await dbContext.PushSubscriptions
            .Where(s => s.UserId == userId.Value && s.Endpoint == request.Endpoint)
            .ExecuteDeleteAsync(ct);

        return Ok();
    }
}

public record PushSubscribeRequest(string Endpoint, string P256dh, string Auth);
public record PushUnsubscribeRequest(string Endpoint);
