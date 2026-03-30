using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Contracts.Extensions;
using Trampline.Core.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Web.Controllers.Base;
using Trampline.Web.Extensions;

namespace Trampline.Web.Controllers;

[Authorize(Roles = "Worker, Admin")]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class WorkerController(
    ILogger<WorkerController> logger,
    IWorkerService workerService,
    IWorkerRepository workerRepository,
    IUserService userService) : BaseApiController
{
    [AllowAnonymous]
    [SwaggerOperation("Количество соискателей")]
    [HttpGet("count")]
    public async Task<IActionResult> GetCountAsync(CancellationToken cancellationToken)
    {
        var count = await workerRepository.CountAsync(cancellationToken);
        return Ok(new { count });
    }

    [AllowAnonymous]
    [SwaggerOperation("Поиск соискателей")]
    [HttpGet]
    public async Task<IActionResult> SearchWorkersAsync(
        string? search,
        string? skills,
        string? university,
        CancellationToken cancellationToken,
        int pageSize = 10,
        int pageNumber = 1)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        pageNumber = Math.Max(pageNumber, 1);
        var (items, totalCount) = await workerRepository.GetAllAsync(
            pageNumber, pageSize, search, skills, university, cancellationToken);

        var responseItems = items.Select(w =>
        {
            var isPrivate = w.User?.IsPrivate ?? false;
            if (isPrivate)
            {
                return new WorkerSearchResponse
                {
                    Id = w.Id,
                    UserId = w.UserId,
                    Name = w.Name,
                    LastName = w.LastName,
                    Patronymic = string.Empty,
                    Photo = w.Photo,
                    IsPrivate = true
                };
            }

            return new WorkerSearchResponse
            {
                Id = w.Id,
                UserId = w.UserId,
                Name = w.Name,
                LastName = w.LastName,
                Patronymic = w.Patronymic ?? string.Empty,
                Photo = w.Photo,
                About = w.About,
                Skills = w.Skills,
                University = w.Info?.University,
                Course = w.Info?.Course,
                IsPrivate = false
            };
        }).ToList();

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
    [SwaggerOperation("Публичный профиль соискателя")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkerProfileAsync(Guid id, CancellationToken cancellationToken)
    {
        var profile = await workerRepository.GetByIdAsync(id, cancellationToken);
        if (profile == null)
            return NotFound();

        var owner = await userService.GetByIdAsync(profile.UserId, cancellationToken);
        if (owner is { IsPrivate: true })
        {
            var requestingUserId = GetUserIdString();
            var isOwner = requestingUserId != null && Guid.Parse(requestingUserId) == owner.Id;
            var isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isAdmin)
                return StatusCode(403, new { message = "Профиль скрыт" });
        }

        return Ok(profile.ToWorkerProfileResponse());
    }

    [AllowAnonymous]
    [SwaggerOperation("Профиль соискателя по userId")]
    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetWorkerByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await workerRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile == null)
            return NotFound();

        var owner = await userService.GetByIdAsync(profile.UserId, cancellationToken);
        if (owner is { IsPrivate: true })
        {
            var requestingUserId = GetUserIdString();
            var isOwner = requestingUserId != null && Guid.Parse(requestingUserId) == owner.Id;
            var isAdmin = User.IsInRole("Admin");

            if (!isOwner && !isAdmin)
                return StatusCode(403, new { message = "Профиль скрыт" });
        }

        return Ok(profile.ToWorkerProfileResponse());
    }

    [HttpPut]
    public async Task<IActionResult> UpdateWorkerProfile(WorkerProfileRequest request, CancellationToken cancellationToken)
    {
        var userGuid = GetUserId();

        var result = await workerService.UpdateProfileAsync(userGuid, request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Worker profile updated by {UserId}", userGuid);
        return Ok(result.Value!.WorkerProfile?.ToWorkerProfileResponse());
    }

    [SwaggerOperation("Загрузка резюме в формате .pdf, .docx, .doc")]
    [EnableRateLimiting("upload")]
    [HttpPost("upload-resume")]
    public async Task<IActionResult> UploadResumeAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var userGuid = GetUserId();

        var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return UnprocessableEntity("Only pdf, docx, doc are allowed");
        }

        var result = await workerService.UpdateResumeAsync(userGuid, file, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Resume uploaded by {UserId}", userGuid);
        return Ok(result.Value!.WorkerProfile?.Resume);
    }

    [SwaggerOperation("Загрузить аватарку")]
    [EnableRateLimiting("upload")]
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadPhotoAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var userGuid = GetUserId();

        var allowedExtensions = new[] { ".jpg", ".webp", ".png" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return UnprocessableEntity("Only .jpg, .webp, .png are allowed");
        }

        var result = await workerService.UpdateAvatarAsync(userGuid, file, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Avatar uploaded by {UserId}", userGuid);
        return Ok(result.Value!.WorkerProfile?.Photo ?? result.Value!.Avatar);
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplicationsAsync(CancellationToken cancellationToken)
    {
        var userGuid = GetUserId();

        var result = await workerService.GetApplicationsAsync(userGuid, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value!.Select(x => x.ToJobApplicationResponse()));
    }

    [HttpGet("event-applications")]
    public async Task<IActionResult> GetEventApplicationsAsync(CancellationToken cancellationToken)
    {
        var userGuid = GetUserId();

        var result = await workerService.GetEventApplicationsAsync(userGuid, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value!.Select(x => x.ToEventApplicationResponse()));
    }

    [HttpGet("mentorship-applications")]
    public async Task<IActionResult> GetMentorshipApplicationsAsync(CancellationToken cancellationToken)
    {
        var userGuid = GetUserId();

        var result = await workerService.GetMentorshipApplicationsAsync(userGuid, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        return Ok(result.Value!.Select(x => x.ToMentorshipApplicationResponse()));
    }
}
