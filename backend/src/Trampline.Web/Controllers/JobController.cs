using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Application.Services.Employees;
using Trampline.Application.Services.IO;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.Extensions;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;
using Trampline.Contracts.DTOs.Responses;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Web.Extensions;

namespace Trampline.Web.Controllers;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class JobController(
    ILogger<JobController> logger,
    IJobService jobService,
    IJobRepository repository,
    INotificationService notificationService,
    IFavoriteRepository favoriteRepository,
    IUserService userService,
    ITagRepository tagRepository,
    IJobApplicationRepository jobApplicationRepository,
    IMediaService mediaService) : ControllerBase
{
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Получить все теги")]
    [HttpGet("tags")]
    public async Task<IActionResult> GetTagsAsync(CancellationToken ct)
    {
        var tags = await tagRepository.GetAllAsync(ct);
        return Ok(tags.Select(t => new { t.Id, t.Name, t.Category, t.Lvl }));
    }

    [AllowAnonymous]
    [SwaggerOperation(Summary = "Статистика тегов по популярности")]
    [HttpGet("tags/stats")]
    public async Task<IActionResult> GetTagStatsAsync(CancellationToken ct)
    {
        var stats = await tagRepository.GetAllWithStatsAsync(ct);
        return Ok(stats.Select(s => new TagStatsResponse
        {
            Id = s.Tag.Id,
            Name = s.Tag.Name,
            Category = s.Tag.Category,
            JobCount = s.JobCount,
            EventCount = s.EventCount,
            TotalCount = s.JobCount + s.EventCount
        }));
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation(Summary = "Создать тег")]
    [HttpPost("tags")]
    public async Task<IActionResult> CreateTagAsync([FromBody] CreateTagRequest request, CancellationToken ct)
    {
        var exists = await tagRepository.ExistsAsync(request.Name, ct);
        if (exists) return Conflict("Tag already exists");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Category = request.Category,
            Lvl = 0
        };

        await tagRepository.AddAsync(tag, ct);

        logger.LogInformation("Tag created by employee: {TagName}", tag.Name);
        return Ok(tag);
    }

    [AllowAnonymous]
    [SwaggerOperation(Summary = "Пагинация", Description = "")]
    [SwaggerResponse(200, "Успешный ответ", typeof(IEnumerable<Job>))]
    [HttpGet]
    public async Task<IActionResult> GetPaginationAsync(string? city, int? salaryMin, int? salaryMax, string? search,
        string? type, string? format, string? tags,
        CancellationToken cancellationToken, int pageSize = 10, int pageNumber = 1)
    {
        pageSize = Math.Clamp(pageSize, 1, 500);
        pageNumber = Math.Max(pageNumber, 1);
        if (search?.Length > 200) search = search[..200];
        var (items, totalCount) = await repository.GetPaginationAsync(
            pageNumber, pageSize, city, salaryMin, salaryMax, search, type, format, tags, cancellationToken);

        var responseItems = items.Select(x => x.ToJobResponse()).ToList();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            var ids = responseItems.Select(x => x.Id).ToList();
            var favIds = await favoriteRepository.GetFavoriteTargetIdsAsync(userId.Value, FavoriteType.Job, ids, cancellationToken);
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
    [SwaggerOperation("Вакансии созданные пользователем")]
    [SwaggerResponse(200, "Успешный ответ", typeof(IEnumerable<Job>))]
    [HttpGet("all-by/{userId}")]
    public async Task<IActionResult> GetByEmployeeAsync(Guid userId, CancellationToken cancellationToken, int pageSize = 10, int pageNumber = 1)
    {
        var list = await repository.GetAllByUserIdAsync(userId, pageNumber, pageSize, cancellationToken);

        return Ok(list.Select(x => x.ToJobResponse()));
    }

    [SwaggerOperation("Создать вакансию")]
    [Authorize(Roles = "Employee")]
    [EnableRateLimiting("write")]
    [HttpPost]
    public async Task<IActionResult> CreateJobAsync(CreateJobRequest request, CancellationToken cancellationToken)
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

        var result = await jobService.CreateJob(new Guid(userId), request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Job created by {UserId}", userId);
        return Ok(result.Value!.ToJobResponse());
    }

    [AllowAnonymous]
    [SwaggerOperation(Summary = "Пакетное получение вакансий по ID")]
    [HttpPost("batch")]
    public async Task<IActionResult> GetBatchAsync([FromBody] BatchIdsRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids.Length == 0) return Ok(Array.Empty<object>());
        if (request.Ids.Length > 50) return BadRequest("Maximum 50 ids per request");

        var dict = await repository.GetByIdsAsync(request.Ids, cancellationToken);
        var responseItems = dict.Values.Select(x => x.ToJobResponse()).ToList();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            var ids = responseItems.Select(x => x.Id).ToList();
            var favIds = await favoriteRepository.GetFavoriteTargetIdsAsync(userId.Value, FavoriteType.Job, ids, cancellationToken);
            foreach (var item in responseItems) item.IsFavorited = favIds.Contains(item.Id);
        }

        return Ok(responseItems);
    }

    [AllowAnonymous]
    [SwaggerResponse(200, "Успешный ответ", typeof(Job))]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userIdLine = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        Result<Job>? find;
        if (!string.IsNullOrEmpty(userIdLine))
        {
            find = await jobService.GetByIdAsync(id, cancellationToken, new Guid(userIdLine));
        }
        else
        {
            find = await jobService.GetByIdAsync(id, cancellationToken);
        }

        if (find == null)
        {
            return NotFound();
        }

        var jobValue = find.Value;
        if (jobValue != null)
        {
            var owner = await userService.GetByIdAsync(jobValue.UserId, cancellationToken);
            if (owner is { IsPrivate: true })
            {
                var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (requesterId == null || new Guid(requesterId) != owner.Id)
                    return NotFound();
            }
        }

        var response = find.Value!.ToJobResponse();

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Guid? userId = userIdClaim != null ? Guid.Parse(userIdClaim) : null;

        if (userId.HasValue)
        {
            response.IsFavorited = await favoriteRepository.IsFavoriteAsync(userId.Value, response.Id, FavoriteType.Job, cancellationToken);
        }

        return Ok(response);
    }

    [Authorize(Roles = "Employee")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateJobAsync(Guid id, UpdateJobRequest request,
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

        var result = await jobService.UpdateAsync(new Guid(userId), id, request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Job {Id} updated by {UserId}", id, userId);
        return Ok(result.Value!.ToJobResponse());
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Получить отклики")]
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

        var result = await jobService.GetApplicationsAsync(new Guid(userId), id, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value!.Select(x => x.ToJobApplicationResponse()));
    }

    [Authorize(Roles = "Worker")]
    [SwaggerOperation("Отклик на вакансию")]
    [HttpPost("application-job")]
    public async Task<IActionResult> ApplicationJobAsync(JobApplicationRequest request, CancellationToken cancellationToken)
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

        var result = await jobService.ApplicationJobAsync(new Guid(userId), request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Application submitted for job by {UserId}", userId);

        try
        {
            var job = result.Value;
            if (job != null)
            {
                await notificationService.SendAsync(job.UserId, "new_application", new
                {
                    jobId = job.Id,
                    jobTitle = job.Title,
                    applicantId = userId
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send new application notification");
        }

        return Ok(result.Value!.ToJobResponse());
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Обновить статус отклика")]
    [HttpPut("application/{applicationId}/status")]
    public async Task<IActionResult> UpdateApplicationStatusAsync(Guid applicationId, [FromBody] UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var result = await jobService.UpdateApplicationStatusAsync(new Guid(userId), applicationId, request.Status, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        logger.LogInformation("Application {AppId} status updated", applicationId);

        try
        {
            var application = await jobApplicationRepository.GetByIdAsync(applicationId, cancellationToken);
            if (application != null)
            {
                await notificationService.SendAsync(application.Profile.UserId, "application_status", new
                {
                    applicationId,
                    jobId = application.JobId,
                    jobTitle = application.Job?.Title,
                    status = request.Status.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send application status notification");
        }

        return Ok();
    }

    [Authorize(Roles = "Employee")]
    [EnableRateLimiting("upload")]
    [SwaggerOperation("Загрузить фото к вакансии")]
    [HttpPost("{id}/photo")]
    public async Task<IActionResult> UploadJobPhotoAsync(Guid id, IFormFile[] files, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });
        if (files.Length == 0) return UnprocessableEntity("File list is empty.");

        var job = await repository.GetByIdAsync(id, ct);
        if (job == null) return NotFound();
        if (job.UserId != new Guid(userId)) return Forbid();

        var allowedExtensions = new[] { ".jpg", ".webp", ".png" };
        foreach (var file in files)
        {
            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                return UnprocessableEntity("Only .jpg, .webp, .png are allowed");
        }

        foreach (var file in files)
        {
            var result = await mediaService.UploadFile(file, ct);
            if (result.IsSuccess) job.AddPhoto(result.Value!);
        }

        await repository.UpdateAsync(job, ct);
        return Ok(job.Photos);
    }

    [Authorize(Roles = "Employee")]
    [EnableRateLimiting("upload")]
    [SwaggerOperation("Загрузить видео к вакансии")]
    [HttpPost("{id}/video")]
    public async Task<IActionResult> UploadJobVideoAsync(Guid id, IFormFile[] files, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });
        if (files.Length == 0) return UnprocessableEntity("File list is empty.");

        var job = await repository.GetByIdAsync(id, ct);
        if (job == null) return NotFound();
        if (job.UserId != new Guid(userId)) return Forbid();

        var allowedExtensions = new[] { ".mp4", ".webm" };
        foreach (var file in files)
        {
            if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                return UnprocessableEntity("Only .mp4, .webm are allowed");
        }

        foreach (var file in files)
        {
            var result = await mediaService.UploadFile(file, ct);
            if (result.IsSuccess) job.AddVideo(result.Value!);
        }

        await repository.UpdateAsync(job, ct);
        return Ok(job.Videos);
    }

    [SwaggerOperation("Удалить фото вакансии")]
    [Authorize(Roles = "Employee, Admin")]
    [EnableRateLimiting("upload")]
    [HttpDelete("{id}/photo")]
    public async Task<IActionResult> DeleteJobPhotoAsync(Guid id, [FromQuery] string path, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var job = await jobService.GetByIdAsync(id, ct);
        if (job.IsFailure) return job.ToActionResult();

        if (job.Value!.UserId != new Guid(userId) && !User.IsInRole("Admin"))
            return Forbid();

        if (!job.Value!.Photos.Contains(path))
            return BadRequest(new ProblemDetails { Title = "Photo not found", Status = 400 });

        var result = await mediaService.DeleteFile(path);
        if (result.IsFailure) return result.ToActionResult();

        job.Value!.Photos.Remove(path);
        await repository.UpdateAsync(job.Value!, ct);

        return Ok();
    }

    [SwaggerOperation("Удалить видео вакансии")]
    [Authorize(Roles = "Employee, Admin")]
    [EnableRateLimiting("upload")]
    [HttpDelete("{id}/video")]
    public async Task<IActionResult> DeleteJobVideoAsync(Guid id, [FromQuery] string path, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var job = await jobService.GetByIdAsync(id, ct);
        if (job.IsFailure) return job.ToActionResult();

        if (job.Value!.UserId != new Guid(userId) && !User.IsInRole("Admin"))
            return Forbid();

        if (!job.Value!.Videos.Contains(path))
            return BadRequest(new ProblemDetails { Title = "Video not found", Status = 400 });

        var result = await mediaService.DeleteFile(path);
        if (result.IsFailure) return result.ToActionResult();

        job.Value!.Videos.Remove(path);
        await repository.UpdateAsync(job.Value!, ct);

        return Ok();
    }

    [Authorize(Roles = "Employee, Admin")]
    [SwaggerOperation("Удалить вакансию")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteJobAsync(Guid id, CancellationToken cancellationToken)
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

        await jobService.DeleteAsync(id, new Guid(userId), cancellationToken);

        logger.LogInformation("Job {Id} deleted by {UserId}", id, userId);
        return Ok();
    }
}