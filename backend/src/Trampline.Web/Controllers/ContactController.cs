using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models.Worker;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Core.Repositories;

namespace Trampline.Web.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class ContactController(
    ILogger<ContactController> logger,
    IContactRepository contactRepository,
    INotificationService notificationService,
    IRecommendationRepository recommendationRepository,
    IWorkerRepository workerRepository,
    IUserService userService,
    IJobRepository jobRepository) : ControllerBase
{
    private Guid? GetUserId()
    {
        var val = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return string.IsNullOrEmpty(val) ? null : new Guid(val);
    }

    [SwaggerOperation("Мои контакты (принятые)")]
    [HttpGet]
    public async Task<IActionResult> GetContactsAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var contacts = await contactRepository.GetContactsAsync(userId.Value, ct);
        var contactUserIds = contacts
            .Select(c => c.RequesterId == userId.Value ? c.ReceiverId : c.RequesterId)
            .ToList();

        var profiles = await workerRepository.GetByUserIdsAsync(contactUserIds, ct);

        return Ok(contacts.Select(c =>
        {
            var contactUserId = c.RequesterId == userId.Value ? c.ReceiverId : c.RequesterId;
            profiles.TryGetValue(contactUserId, out var profile);
            return new ContactResponse
            {
                Id = c.Id,
                ContactUserId = contactUserId,
                Name = profile?.Name ?? string.Empty,
                LastName = profile?.LastName ?? string.Empty,
                Patronymic = profile?.Patronymic ?? string.Empty,
                Photo = profile?.Photo,
                Skills = profile?.Skills ?? [],
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            };
        }));
    }

    [SwaggerOperation("Входящие заявки")]
    [HttpGet("incoming")]
    public async Task<IActionResult> GetIncomingAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var incoming = await contactRepository.GetIncomingAsync(userId.Value, ct);
        var requesterIds = incoming.Select(c => c.RequesterId).ToList();

        var profiles = await workerRepository.GetByUserIdsAsync(requesterIds, ct);

        return Ok(incoming.Select(c =>
        {
            profiles.TryGetValue(c.RequesterId, out var profile);
            return new ContactResponse
            {
                Id = c.Id,
                ContactUserId = c.RequesterId,
                Name = profile?.Name ?? string.Empty,
                LastName = profile?.LastName ?? string.Empty,
                Patronymic = profile?.Patronymic ?? string.Empty,
                Photo = profile?.Photo,
                Skills = profile?.Skills ?? [],
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            };
        }));
    }

    [SwaggerOperation("Отправить заявку в контакты")]
    [HttpPost("{receiverId}")]
    public async Task<IActionResult> SendRequestAsync(Guid receiverId, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();
        if (userId.Value == receiverId)
            return BadRequest(new ProblemDetails { Title = "Cannot add yourself", Status = 400 });

        var existing = await contactRepository.GetByPairAsync(userId.Value, receiverId, ct);
        if (existing != null)
            return Conflict(new ProblemDetails { Title = "Contact request already exists", Status = 409 });

        var receiver = await userService.GetByIdAsync(receiverId, ct);
        if (receiver == null)
            return NotFound(new ProblemDetails { Title = "User not found", Status = 404 });

        var contact = Contact.Create(userId.Value, receiverId);
        try
        {
            await contactRepository.AddAsync(contact, ct);
        }
        catch (Exception ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            return Conflict(new ProblemDetails { Title = "Contact request already exists", Status = 409 });
        }
        logger.LogInformation("Contact request sent from {UserId} to {ReceiverId}", userId.Value, receiverId);

        try
        {
            await notificationService.SendAsync(receiverId, "contact_request", new
            {
                contactId = contact.Id,
                fromUserId = userId.Value
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send contact request notification");
        }

        return Ok(contact.Id);
    }

    [SwaggerOperation("Принять заявку")]
    [HttpPut("{id}/accept")]
    public async Task<IActionResult> AcceptAsync(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var contact = await contactRepository.GetByIdAsync(id, ct);
        if (contact == null) return NotFound();
        if (contact.ReceiverId != userId.Value) return Forbid();

        contact.Accept();
        await contactRepository.UpdateAsync(contact, ct);
        logger.LogInformation("Contact request {Id} accepted", id);
        return Ok();
    }

    [SwaggerOperation("Отклонить заявку")]
    [HttpPut("{id}/decline")]
    public async Task<IActionResult> DeclineAsync(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var contact = await contactRepository.GetByIdAsync(id, ct);
        if (contact == null) return NotFound();
        if (contact.ReceiverId != userId.Value) return Forbid();

        contact.Decline();
        await contactRepository.UpdateAsync(contact, ct);
        logger.LogInformation("Contact request {Id} declined", id);
        return Ok();
    }

    [SwaggerOperation("Удалить контакт")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var contact = await contactRepository.GetByIdAsync(id, ct);
        if (contact == null) return NotFound();
        if (contact.RequesterId != userId.Value && contact.ReceiverId != userId.Value) return Forbid();

        await contactRepository.DeleteAsync(id, ct);
        logger.LogInformation("Contact {Id} deleted", id);
        return Ok();
    }

    [SwaggerOperation("Рекомендовать контакт на вакансию")]
    [HttpPost("recommend")]
    public async Task<IActionResult> RecommendAsync([FromBody] RecommendRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var job = await jobRepository.GetByIdAsync(request.JobId, ct);
        if (job == null || job.DeletedAt != null || !job.IsActive)
            return BadRequest(new ProblemDetails { Title = "Job not found or inactive", Status = 400 });

        var recommendation = Recommendation.Create(userId.Value, request.ToUserId, request.JobId, request.Message);
        await recommendationRepository.AddAsync(recommendation, ct);

        logger.LogInformation("User {UserId} recommended contact for job {JobId}", userId.Value, request.JobId);

        try
        {
            await notificationService.SendAsync(request.ToUserId, "job_recommendation", new
            {
                recommendationId = recommendation.Id,
                fromUserId = userId.Value,
                jobId = request.JobId,
                message = request.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send job recommendation notification");
        }

        return Ok(recommendation.Id);
    }

    [SwaggerOperation("Мои входящие рекомендации")]
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetRecommendationsAsync(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == null) return BadRequest();

        var recs = await recommendationRepository.GetByReceiverAsync(userId.Value, ct);

        var fromUserIds = recs.Select(r => r.FromUserId).Distinct();
        var jobIds = recs.Select(r => r.JobId).Distinct();

        var users = await userService.GetByIdsAsync(fromUserIds, ct);
        var jobs = await jobRepository.GetByIdsAsync(jobIds, ct);

        return Ok(recs.Select(r =>
        {
            users.TryGetValue(r.FromUserId, out var fromUser);
            jobs.TryGetValue(r.JobId, out var job);
            return new RecommendationResponse
            {
                Id = r.Id,
                FromUserId = r.FromUserId,
                FromUserName = fromUser?.Nickname ?? string.Empty,
                JobId = r.JobId,
                JobTitle = job?.Title ?? string.Empty,
                CompanyName = job?.Employee?.Name ?? string.Empty,
                Message = r.Message,
                CreatedAt = r.CreatedAt
            };
        }));
    }
}
