using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Application.Services.Events;
using Trampline.Application.Services.IO;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.Extensions;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Core.Constants;
using Trampline.Web.Extensions;

namespace Trampline.Web.Controllers;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class EventController(
    ILogger<EventController> logger,
    IEventRepository repository,
    IEventService eventService,
    INotificationService notificationService,
    IFavoriteRepository favoriteRepository,
    IUserService userService,
    IEventApplicationRepository eventApplicationRepository,
    IMediaService mediaService) : ControllerBase
{
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Пагинация", Description = "")]
    [SwaggerResponse(200, "Успешный ответ", typeof(IEnumerable<Event>))]
    [HttpGet]
    public async Task<IActionResult> GetPaginationAsync(string? city, int? salaryMin, int? salaryMax, string? search,
        string? format, string? tags,
        CancellationToken cancellationToken, int pageSize = 10, int pageNumber = 1)
    {
        pageSize = Math.Clamp(pageSize, 1, 500);
        pageNumber = Math.Max(pageNumber, 1);
        if (search?.Length > 200) search = search[..200];
        var (items, totalCount) = await repository.GetPaginationAsync(
            pageNumber, pageSize, city, salaryMin, salaryMax, search, format, tags, cancellationToken);

        var responseItems = items.Select(x => x.ToEventResponse()).ToList();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            var ids = responseItems.Select(x => x.Id).ToList();
            var favIds = await favoriteRepository.GetFavoriteTargetIdsAsync(userId.Value, FavoriteType.Event, ids, cancellationToken);
            foreach (var item in responseItems) item.IsFavorited = favIds.Contains(item.Id);
        }

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new
        {
            items = responseItems,
            totalCount,
            totalPages,
            pageSize,
            currentPage = pageNumber,
            hasNextPage = pageNumber < totalPages,
            hasPreviousPage = pageNumber > 1
        });
    }

    [AllowAnonymous]
    [SwaggerOperation("Мероприятия созданные пользователем")]
    [SwaggerResponse(200, "Успешный ответ", typeof(IEnumerable<Event>))]
    [HttpGet("all-by/{userId}")]
    public async Task<IActionResult> GetByEmployeeAsync(Guid userId, CancellationToken cancellationToken, int pageSize = 10, int pageNumber = 1)
    {
        var list = await repository.GetAllByUserIdAsync(userId, pageNumber, pageSize, cancellationToken);

        return Ok(list.Select(x => x.ToEventResponse()));
    }

    [Authorize(Roles = "Employee")]
    [SwaggerOperation("Создать мероприятие")]
    [EnableRateLimiting("write")]
    [HttpPost]
    public async Task<IActionResult> CreateEventAsync(CreateEventRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new ProblemDetails()
            {
                Title = "token is invalid",
                Status = 400,
                Detail = "Please provide a valid token"
            });
        }

        var result = await eventService.CreateEventAsync(new Guid(userId), request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Event created by {UserId}", userId);
        return Ok(result.Value!.Id);
    }

    [AllowAnonymous]
    [SwaggerOperation(Summary = "Пакетное получение мероприятий по ID")]
    [HttpPost("batch")]
    public async Task<IActionResult> GetBatchAsync([FromBody] BatchIdsRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids.Length == 0) return Ok(Array.Empty<object>());
        if (request.Ids.Length > 50) return BadRequest("Maximum 50 ids per request");

        var dict = await repository.GetByIdsAsync(request.Ids, cancellationToken);
        var responseItems = dict.Values.Select(x => x.ToEventResponse()).ToList();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            var ids = responseItems.Select(x => x.Id).ToList();
            var favIds = await favoriteRepository.GetFavoriteTargetIdsAsync(userId.Value, FavoriteType.Event, ids, cancellationToken);
            foreach (var item in responseItems) item.IsFavorited = favIds.Contains(item.Id);
        }

        return Ok(responseItems);
    }

    [AllowAnonymous]
    [SwaggerResponse(200, "Успешный ответ", typeof(Event))]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEventByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userIdLine = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        Result<Event>? find;
        if (!string.IsNullOrEmpty(userIdLine))
        {
            find = await eventService.GetByIdAsync(id, cancellationToken, new Guid(userIdLine));
        }
        else
        {
            find = await eventService.GetByIdAsync(id, cancellationToken);
        }

        if (find == null)
        {
            return NotFound();
        }

        var eventValue = find.Value;
        if (eventValue != null)
        {
            var owner = await userService.GetByIdAsync(eventValue.UserId, cancellationToken);
            if (owner is { IsPrivate: true })
            {
                var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (requesterId == null || new Guid(requesterId) != owner.Id)
                    return NotFound();
            }
        }

        var response = find.Value!.ToEventResponse();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            response.IsFavorited = await favoriteRepository.IsFavoriteAsync(userId.Value, response.Id, FavoriteType.Event, cancellationToken);
        }

        return Ok(response);
    }

    [Authorize(Roles = "Employee")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEventAsync(Guid id, UpdateEventRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new ProblemDetails()
            {
                Title = "token is invalid",
                Status = 400,
                Detail = "Please provide a valid token"
            });
        }

        var result = await eventService.UpdateAsync(new Guid(userId), id, request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Event {Id} updated by {UserId}", id, userId);
        return Ok(result.Value!.Id);
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Получить отклики на мероприятие")]
    [HttpGet("{id}/responses")]
    public async Task<IActionResult> GetApplicationJobAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new ProblemDetails()
            {
                Title = "token is invalid",
                Status = 400,
                Detail = "Please provide a valid token"
            });
        }

        var result = await eventService.GetApplicationsAsync(new Guid(userId), id, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value!.Select(x => x.ToEventApplicationResponse()));
    }

    [Authorize(Roles = "Worker")]
    [SwaggerOperation("Отклик на мероприятие")]
    [HttpPost("application-event")]
    public async Task<IActionResult> ApplicationEventAsync(EventApplicationRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new ProblemDetails()
            {
                Title = "token is invalid",
                Status = 400,
                Detail = "Please provide a valid token"
            });
        }

        var result = await eventService.ApplicationEventAsync(new Guid(userId), request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Event application submitted by {UserId}", userId);

        try
        {
            var evt = result.Value;
            if (evt != null)
            {
                await notificationService.SendAsync(evt.UserId, NotificationTypes.NewApplication, new
                {
                    eventId = evt.Id,
                    eventTitle = evt.Title,
                    applicantId = userId
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send new event application notification");
        }

        return Ok(result.Value!.Id);
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Обновить статус отклика на мероприятие")]
    [HttpPut("application/{applicationId}/status")]
    public async Task<IActionResult> UpdateApplicationStatusAsync(Guid applicationId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var result = await eventService.UpdateApplicationStatusAsync(new Guid(userId), applicationId, request.Status, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        logger.LogInformation("Event application {AppId} status updated", applicationId);

        try
        {
            var application = await eventApplicationRepository.GetByIdAsync(applicationId, cancellationToken);
            if (application != null)
            {
                await notificationService.SendAsync(application.Profile.UserId, NotificationTypes.ApplicationStatus, new
                {
                    applicationId,
                    eventId = application.EventId,
                    eventTitle = application.Event?.Title,
                    status = request.Status.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send event application status notification");
        }

        return Ok();
    }

    [Authorize(Roles = "Employee")]
    [EnableRateLimiting("upload")]
    [SwaggerOperation("Загрузить фото к мероприятию")]
    [HttpPost("{id}/photo")]
    public async Task<IActionResult> UploadEventPhotoAsync(Guid id, IFormFile[] files, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });
        var photoError = files.ValidatePhotos();
        if (photoError != null) return UnprocessableEntity(photoError);

        var evt = await repository.GetByIdAsync(id, ct);
        if (evt == null) return NotFound();
        if (evt.UserId != new Guid(userId)) return Forbid();

        foreach (var file in files)
        {
            var result = await mediaService.UploadFile(file, ct);
            if (result.IsSuccess) evt.AddPhoto(result.Value!);
        }

        await repository.UpdateAsync(evt, ct);
        return Ok(evt.Photos);
    }

    [Authorize(Roles = "Employee")]
    [EnableRateLimiting("upload")]
    [SwaggerOperation("Загрузить видео к мероприятию")]
    [HttpPost("{id}/video")]
    public async Task<IActionResult> UploadEventVideoAsync(Guid id, IFormFile[] files, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var videoError = files.ValidateVideos();
        if (videoError != null) return UnprocessableEntity(videoError);

        var evt = await repository.GetByIdAsync(id, ct);
        if (evt == null) return NotFound();
        if (evt.UserId != new Guid(userId)) return Forbid();

        foreach (var file in files)
        {
            var result = await mediaService.UploadFile(file, ct);
            if (result.IsSuccess) evt.AddVideo(result.Value!);
        }

        await repository.UpdateAsync(evt, ct);
        return Ok(evt.Videos);
    }

    [SwaggerOperation("Удалить фото мероприятия")]
    [Authorize(Roles = "Employee, Admin")]
    [EnableRateLimiting("upload")]
    [HttpDelete("{id}/photo")]
    public async Task<IActionResult> DeleteEventPhotoAsync(Guid id, [FromQuery] string path, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var evt = await eventService.GetByIdAsync(id, ct);
        if (evt.IsFailure) return evt.ToActionResult();

        if (evt.Value!.UserId != new Guid(userId) && !User.IsInRole("Admin"))
            return Forbid();

        if (!evt.Value!.Photos.Contains(path))
            return BadRequest(new ProblemDetails { Title = "Photo not found", Status = 400 });

        var result = await mediaService.DeleteFile(path);
        if (result.IsFailure) return result.ToActionResult();

        evt.Value!.Photos.Remove(path);
        await repository.UpdateAsync(evt.Value!, ct);

        return Ok();
    }

    [SwaggerOperation("Удалить видео мероприятия")]
    [Authorize(Roles = "Employee, Admin")]
    [EnableRateLimiting("upload")]
    [HttpDelete("{id}/video")]
    public async Task<IActionResult> DeleteEventVideoAsync(Guid id, [FromQuery] string path, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var evt = await eventService.GetByIdAsync(id, ct);
        if (evt.IsFailure) return evt.ToActionResult();

        if (evt.Value!.UserId != new Guid(userId) && !User.IsInRole("Admin"))
            return Forbid();

        if (!evt.Value!.Videos.Contains(path))
            return BadRequest(new ProblemDetails { Title = "Video not found", Status = 400 });

        var result = await mediaService.DeleteFile(path);
        if (result.IsFailure) return result.ToActionResult();

        evt.Value!.Videos.Remove(path);
        await repository.UpdateAsync(evt.Value!, ct);

        return Ok();
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Удалить мероприятие")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEventAsync(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new ProblemDetails()
            {
                Title = "token is invalid",
                Status = 400,
                Detail = "Please provide a valid token"
            });
        }

        await eventService.DeleteAsync(id, new Guid(userId), cancellationToken);

        logger.LogInformation("Event {Id} deleted by {UserId}", id, userId);
        return Ok();
    }
}