using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Application.Services.Mentorships;
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
public class MentorshipController(
    ILogger<MentorshipController> logger,
    IMentorshipRepository repository,
    IMentorshipService mentorshipService,
    INotificationService notificationService,
    IFavoriteRepository favoriteRepository,
    IUserService userService,
    IMentorshipApplicationRepository mentorshipApplicationRepository,
    IMediaService mediaService) : ControllerBase
{
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Пагинация", Description = "")]
    [SwaggerResponse(200, "Успешный ответ", typeof(IEnumerable<Mentorship>))]
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

        var responseItems = items.Select(x => x.ToMentorshipResponse()).ToList();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            var ids = responseItems.Select(x => x.Id).ToList();
            var favIds = await favoriteRepository.GetFavoriteTargetIdsAsync(userId.Value, FavoriteType.Mentorship, ids, cancellationToken);
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
    [SwaggerOperation("Менторства созданные пользователем")]
    [SwaggerResponse(200, "Успешный ответ", typeof(IEnumerable<Mentorship>))]
    [HttpGet("all-by/{userId}")]
    public async Task<IActionResult> GetByEmployeeAsync(Guid userId, CancellationToken cancellationToken, int pageSize = 10, int pageNumber = 1)
    {
        var list = await repository.GetAllByUserIdAsync(userId, pageNumber, pageSize, cancellationToken);

        return Ok(list.Select(x => x.ToMentorshipResponse()));
    }

    [Authorize(Roles = "Employee")]
    [SwaggerOperation("Создать менторство")]
    [EnableRateLimiting("write")]
    [HttpPost]
    public async Task<IActionResult> CreateMentorshipAsync(CreateMentorshipRequest request, CancellationToken cancellationToken)
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

        var result = await mentorshipService.CreateMentorshipAsync(new Guid(userId), request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Mentorship created by {UserId}", userId);
        return Ok(result.Value!.Id);
    }

    [AllowAnonymous]
    [SwaggerOperation(Summary = "Пакетное получение менторств по ID")]
    [HttpPost("batch")]
    public async Task<IActionResult> GetBatchAsync([FromBody] BatchIdsRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids.Length == 0) return Ok(Array.Empty<object>());
        if (request.Ids.Length > 50) return BadRequest("Maximum 50 ids per request");

        var dict = await repository.GetByIdsAsync(request.Ids, cancellationToken);
        var responseItems = dict.Values.Select(x => x.ToMentorshipResponse()).ToList();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            var ids = responseItems.Select(x => x.Id).ToList();
            var favIds = await favoriteRepository.GetFavoriteTargetIdsAsync(userId.Value, FavoriteType.Mentorship, ids, cancellationToken);
            foreach (var item in responseItems) item.IsFavorited = favIds.Contains(item.Id);
        }

        return Ok(responseItems);
    }

    [AllowAnonymous]
    [SwaggerResponse(200, "Успешный ответ", typeof(Mentorship))]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMentorshipByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userIdLine = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        Result<Mentorship>? find;
        if (!string.IsNullOrEmpty(userIdLine))
        {
            find = await mentorshipService.GetByIdAsync(id, cancellationToken, new Guid(userIdLine));
        }
        else
        {
            find = await mentorshipService.GetByIdAsync(id, cancellationToken);
        }

        if (find == null)
        {
            return NotFound();
        }

        var mentorshipValue = find.Value;
        if (mentorshipValue != null)
        {
            var owner = await userService.GetByIdAsync(mentorshipValue.UserId, cancellationToken);
            if (owner is { IsPrivate: true })
            {
                var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (requesterId == null || new Guid(requesterId) != owner.Id)
                    return NotFound();
            }
        }

        var response = find.Value!.ToMentorshipResponse();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            response.IsFavorited = await favoriteRepository.IsFavoriteAsync(userId.Value, response.Id, FavoriteType.Mentorship, cancellationToken);
        }

        return Ok(response);
    }

    [Authorize(Roles = "Employee")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMentorshipAsync(Guid id, UpdateMentorshipRequest request,
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

        var result = await mentorshipService.UpdateAsync(new Guid(userId), id, request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Mentorship {Id} updated by {UserId}", id, userId);
        return Ok(result.Value!.Id);
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Получить отклики на менторство")]
    [HttpGet("{id}/responses")]
    public async Task<IActionResult> GetApplicationMentorshipAsync(Guid id, CancellationToken cancellationToken)
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

        var result = await mentorshipService.GetApplicationsAsync(new Guid(userId), id, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value!.Select(x => x.ToMentorshipApplicationResponse()));
    }

    [Authorize(Roles = "Worker")]
    [SwaggerOperation("Отклик на менторство")]
    [HttpPost("application-mentorship")]
    public async Task<IActionResult> ApplicationMentorshipAsync(MentorshipApplicationRequest request, CancellationToken cancellationToken)
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

        var result = await mentorshipService.ApplicationMentorshipAsync(new Guid(userId), request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Mentorship application submitted by {UserId}", userId);

        try
        {
            var mentorship = result.Value;
            if (mentorship != null)
            {
                await notificationService.SendAsync(mentorship.UserId, NotificationTypes.NewApplication, new
                {
                    mentorshipId = mentorship.Id,
                    mentorshipTitle = mentorship.Title,
                    applicantId = userId
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send new mentorship application notification");
        }

        return Ok(result.Value!.Id);
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Обновить статус отклика на менторство")]
    [HttpPut("application/{applicationId}/status")]
    public async Task<IActionResult> UpdateApplicationStatusAsync(Guid applicationId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var result = await mentorshipService.UpdateApplicationStatusAsync(new Guid(userId), applicationId, request.Status, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        logger.LogInformation("Mentorship application {AppId} status updated", applicationId);

        try
        {
            var application = await mentorshipApplicationRepository.GetByIdAsync(applicationId, cancellationToken);
            if (application != null)
            {
                await notificationService.SendAsync(application.Profile.UserId, NotificationTypes.ApplicationStatus, new
                {
                    applicationId,
                    mentorshipId = application.MentorshipId,
                    mentorshipTitle = application.Mentorship?.Title,
                    status = request.Status.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send mentorship application status notification");
        }

        return Ok();
    }

    [Authorize(Roles = "Employee")]
    [EnableRateLimiting("upload")]
    [SwaggerOperation("Загрузить фото к менторству")]
    [HttpPost("{id}/photo")]
    public async Task<IActionResult> UploadMentorshipPhotoAsync(Guid id, IFormFile[] files, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });
        var photoError = files.ValidatePhotos();
        if (photoError != null) return UnprocessableEntity(photoError);

        var mentorship = await repository.GetByIdAsync(id, ct);
        if (mentorship == null) return NotFound();
        if (mentorship.UserId != new Guid(userId)) return Forbid();

        foreach (var file in files)
        {
            var result = await mediaService.UploadFile(file, ct);
            if (result.IsSuccess) mentorship.AddPhoto(result.Value!);
        }

        await repository.UpdateAsync(mentorship, ct);
        return Ok(mentorship.Photos);
    }

    [Authorize(Roles = "Employee")]
    [EnableRateLimiting("upload")]
    [SwaggerOperation("Загрузить видео к менторству")]
    [HttpPost("{id}/video")]
    public async Task<IActionResult> UploadMentorshipVideoAsync(Guid id, IFormFile[] files, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var videoError = files.ValidateVideos();
        if (videoError != null) return UnprocessableEntity(videoError);

        var mentorship = await repository.GetByIdAsync(id, ct);
        if (mentorship == null) return NotFound();
        if (mentorship.UserId != new Guid(userId)) return Forbid();

        foreach (var file in files)
        {
            var result = await mediaService.UploadFile(file, ct);
            if (result.IsSuccess) mentorship.AddVideo(result.Value!);
        }

        await repository.UpdateAsync(mentorship, ct);
        return Ok(mentorship.Videos);
    }

    [SwaggerOperation("Удалить фото менторства")]
    [Authorize(Roles = "Employee, Admin")]
    [EnableRateLimiting("upload")]
    [HttpDelete("{id}/photo")]
    public async Task<IActionResult> DeleteMentorshipPhotoAsync(Guid id, [FromQuery] string path, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var mentorship = await mentorshipService.GetByIdAsync(id, ct);
        if (mentorship.IsFailure) return mentorship.ToActionResult();

        if (mentorship.Value!.UserId != new Guid(userId) && !User.IsInRole("Admin"))
            return Forbid();

        if (!mentorship.Value!.Photos.Contains(path))
            return BadRequest(new ProblemDetails { Title = "Photo not found", Status = 400 });

        var result = await mediaService.DeleteFile(path);
        if (result.IsFailure) return result.ToActionResult();

        mentorship.Value!.Photos.Remove(path);
        await repository.UpdateAsync(mentorship.Value!, ct);

        return Ok();
    }

    [SwaggerOperation("Удалить видео менторства")]
    [Authorize(Roles = "Employee, Admin")]
    [EnableRateLimiting("upload")]
    [HttpDelete("{id}/video")]
    public async Task<IActionResult> DeleteMentorshipVideoAsync(Guid id, [FromQuery] string path, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var mentorship = await mentorshipService.GetByIdAsync(id, ct);
        if (mentorship.IsFailure) return mentorship.ToActionResult();

        if (mentorship.Value!.UserId != new Guid(userId) && !User.IsInRole("Admin"))
            return Forbid();

        if (!mentorship.Value!.Videos.Contains(path))
            return BadRequest(new ProblemDetails { Title = "Video not found", Status = 400 });

        var result = await mediaService.DeleteFile(path);
        if (result.IsFailure) return result.ToActionResult();

        mentorship.Value!.Videos.Remove(path);
        await repository.UpdateAsync(mentorship.Value!, ct);

        return Ok();
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Удалить менторство")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMentorshipAsync(Guid id, CancellationToken cancellationToken)
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

        await mentorshipService.DeleteAsync(id, new Guid(userId), cancellationToken);

        logger.LogInformation("Mentorship {Id} deleted by {UserId}", id, userId);
        return Ok();
    }
}
