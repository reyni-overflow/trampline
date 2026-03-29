using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Application.Services.Employees;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Application.Services.IO;
using Trampline.Core.Repositories;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Web.Extensions;

namespace Trampline.Web.Controllers;

[Authorize(Roles = "Employee, Admin")]
[ApiController]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class EmployeeController(
    ILogger<EmployeeController> logger,
    IEmployeeService employeeService,
    IEmployeeRepository employeeRepository,
    IUserService userService,
    IMediaService mediaService) : ControllerBase
{
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Пагинация", Description = "Появляются только Profile с Active - true, верифицрованные")]
    [SwaggerResponse(200, "Успешный ответ", typeof(IEnumerable<EmployeeProfileResponse>))]
    [HttpGet]
    public async Task<IActionResult> GetPaginationAsync(CancellationToken cancellationToken, int pageSize = 10, int pageNumber = 1)
    {
        pageSize = Math.Clamp(pageSize, 1, 500);
        pageNumber = Math.Max(pageNumber, 1);
        var list = await employeeRepository.GetPaginationAsync(pageNumber, pageSize, cancellationToken);

        var filtered = new List<EmployeeProfileResponse>();
        foreach (var e in list.Item1)
        {
            var user = await userService.GetByIdAsync(e.UserId, cancellationToken);
            if (user is { IsPrivate: true }) continue;
            filtered.Add(new EmployeeProfileResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                Name = e.Name,
                Description = e.Description,
                Activity = e.Activity,
                Link = e.Link,
                Socials = e.Socials,
                Photos = e.Photos,
                Videos = e.Videos,
                IsVerified = e.IsVerified,
                VerificationLevel = e.VerificationLevel,
                VerifiedName = e.VerifiedName,
                Info = e.Info
            });
        }

        return Ok(new
        {
            items = filtered,
            total = filtered.Count
        });
    }

    [EnableRateLimiting("upload")]
    [HttpPost("video")]
    public async Task<IActionResult> UploadVideoAsync(IFormFile[] files, CancellationToken cancellationToken)
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

        if (files.Length == 0)
        {
            return UnprocessableEntity("File list is empty.");
        }

        var allowedExtensions = new[] { ".mp4", ".webm" };

        foreach (var file in files)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return UnprocessableEntity("Only .mp4, .webm are allowed");
            }
        }

        var result = await employeeService.UpdateVideoAsync(new Guid(userId), files, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Files uploaded by {UserId}", userId);
        return Ok(result.Value!.EmployeeProfile!.Videos);
    }

    [EnableRateLimiting("upload")]
    [HttpPost("photo")]
    public async Task<IActionResult> UploadPhotoAsync(IFormFile[] files, CancellationToken cancellationToken)
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

        if (files.Length == 0)
        {
            return UnprocessableEntity("File list is empty.");
        }

        var allowedExtensions = new[] { ".jpg", ".webp", ".png" };

        foreach (var file in files)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                return UnprocessableEntity("Only .jpg, .webp, .png are allowed");
            }
        }

        var result = await employeeService.UpdatePhotosAsync(new Guid(userId), files, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Files uploaded by {UserId}", userId);
        return Ok(result.Value!.EmployeeProfile!.Photos);
    }

    [SwaggerOperation("Удалить фото компании")]
    [Authorize(Roles = "Employee, Admin")]
    [HttpDelete("photo")]
    public async Task<IActionResult> DeletePhotoAsync([FromQuery] string path, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profile = await employeeRepository.GetByUserIdAsync(new Guid(userId), ct);
        if (profile == null) return NotFound();

        if (!profile.Photos.Contains(path))
            return BadRequest(new ProblemDetails { Title = "Photo not found in profile", Status = 400 });

        var result = await mediaService.DeleteFile(path);
        if (result.IsFailure)
            logger.LogWarning("File not found on disk, removing from profile: {Path}", path);

        profile.Photos.Remove(path);
        await employeeRepository.UpdateAsync(profile, ct);

        return Ok();
    }

    [SwaggerOperation("Удалить видео компании")]
    [Authorize(Roles = "Employee, Admin")]
    [HttpDelete("video")]
    public async Task<IActionResult> DeleteVideoAsync([FromQuery] string path, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profile = await employeeRepository.GetByUserIdAsync(new Guid(userId), ct);
        if (profile == null) return NotFound();

        if (!profile.Videos.Contains(path))
            return BadRequest(new ProblemDetails { Title = "Video not found in profile", Status = 400 });

        var result = await mediaService.DeleteFile(path);
        if (result.IsFailure) return result.ToActionResult();

        profile.Videos.Remove(path);
        await employeeRepository.UpdateAsync(profile, ct);

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfileAsync(EmployeeProfileRequest request,
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

        var result = await employeeService.UpdateProfileAsync(new Guid(userId), request, cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Employee profile updated by {UserId}", userId);
        var profile = result.Value!.EmployeeProfile;
        return Ok(profile != null ? new EmployeeProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Activity = profile.Activity,
            Description = profile.Description,
            Name = profile.Name,
            Photos = profile.Photos,
            IsVerified = profile.IsVerified,
            VerificationLevel = profile.VerificationLevel,
            VerifiedName = profile.VerifiedName,
            Link = profile.Link,
            Socials = profile.Socials,
            Videos = profile.Videos,
            Info = profile.Info
        } : null);
    }

    [AllowAnonymous]
    [SwaggerOperation(Summary = "Пакетное получение компаний по ID")]
    [HttpPost("batch")]
    public async Task<IActionResult> GetBatchAsync([FromBody] BatchIdsRequest request, CancellationToken cancellationToken)
    {
        if (request.Ids.Length == 0) return Ok(Array.Empty<object>());
        if (request.Ids.Length > 50) return BadRequest("Maximum 50 ids per request");

        var dict = await employeeRepository.GetByIdsAsync(request.Ids, cancellationToken);
        var responseItems = dict.Values.Select(e => new EmployeeProfileResponse
        {
            Id = e.Id,
            UserId = e.UserId,
            Name = e.Name,
            Description = e.Description,
            Activity = e.Activity,
            Link = e.Link,
            Socials = e.Socials,
            Photos = e.Photos,
            Videos = e.Videos,
            IsVerified = e.IsVerified,
            VerificationLevel = e.VerificationLevel,
            Info = e.Info
        }).ToList();

        return Ok(responseItems);
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployeeAsync(Guid id, CancellationToken cancellationToken)
    {
        var findEmployee = await employeeRepository.GetByIdAsync(id, cancellationToken);

        if (findEmployee == null)
            return NotFound();

        var user = await userService.GetByIdAsync(findEmployee.UserId, cancellationToken);
        if (user is { IsPrivate: true })
        {
            var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (requesterId == null || new Guid(requesterId) != user.Id)
                return NotFound();
        }

        return Ok(new EmployeeProfileResponse
        {
            Id = findEmployee.Id,
            UserId = findEmployee.UserId,
            Name = findEmployee.Name,
            Description = findEmployee.Description,
            Activity = findEmployee.Activity,
            Link = findEmployee.Link,
            Socials = findEmployee.Socials,
            Photos = findEmployee.Photos,
            Videos = findEmployee.Videos,
            IsVerified = findEmployee.IsVerified,
            VerificationLevel = findEmployee.VerificationLevel,
            VerifiedName = findEmployee.VerifiedName,
            Info = findEmployee.Info
        });
    }

    [AllowAnonymous]
    [HttpGet("by-user-id/{id}")]
    public async Task<IActionResult> GetEmployeeByUserIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id, cancellationToken);

        if (findUser == null)
        {
            return NotFound();
        }

        if (findUser.IsPrivate)
        {
            var requesterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (requesterId == null || new Guid(requesterId) != findUser.Id)
                return NotFound();
        }

        var profile = findUser.EmployeeProfile;
        if (profile == null) return NotFound();

        return Ok(new EmployeeProfileResponse
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Name = profile.Name,
            Description = profile.Description,
            Activity = profile.Activity,
            Link = profile.Link,
            Socials = profile.Socials,
            Photos = profile.Photos,
            Videos = profile.Videos,
            IsVerified = profile.IsVerified,
            VerificationLevel = profile.VerificationLevel,
            VerifiedName = profile.VerifiedName,
            Info = profile.Info
        });
    }

    [HttpGet("verify")]
    public async Task<IActionResult> ConfirmAsync(CancellationToken cancellationToken)
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

        var result = await employeeService.VerifyCompanyAsync(new Guid(userId), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        logger.LogInformation("Verification requested by {UserId}", userId);
        return Ok(result.Value!);
    }

}