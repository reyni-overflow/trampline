using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Core.Repositories;

namespace Trampline.Web.Controllers;

[ApiController]
[Route("[controller]")]
[EnableRateLimiting("write")]
public class ReviewController(
    ILogger<ReviewController> logger,
    IReviewRepository reviewRepository,
    IUserService userService,
    IWorkerRepository workerRepository,
    IEmployeeRepository employeeRepository) : ControllerBase
{
    [AllowAnonymous]
    [SwaggerOperation("Получить одобренные отзывы")]
    [HttpGet]
    public async Task<IActionResult> GetApprovedAsync(CancellationToken ct)
    {
        var reviews = await reviewRepository.GetApprovedAsync(ct);
        return Ok(reviews.Select(ToResponse));
    }

    [Authorize(Roles = "Admin")]
    [SwaggerOperation("Получить все отзывы")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllAsync(CancellationToken ct)
    {
        var reviews = await reviewRepository.GetAllAsync(ct);
        return Ok(reviews.Select(ToResponse));
    }

    [Authorize]
    [SwaggerOperation("Создать отзыв")]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateReviewRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(new ProblemDetails { Title = "Рейтинг должен быть от 1 до 5", Status = 400 });

        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new ProblemDetails { Title = "Текст отзыва обязателен", Status = 400 });

        var guid = new Guid(userId);
        var user = await userService.GetByIdAsync(guid, ct);
        if (user == null)
            return NotFound();

        var authorName = user.Nickname;
        var authorRole = string.Empty;

        var workerProfile = await workerRepository.GetByUserIdAsync(guid, ct);
        if (workerProfile != null)
        {
            authorName = $"{workerProfile.Name} {workerProfile.LastName}";
            if (workerProfile.Info != null)
                authorRole = $"Студент, {workerProfile.Info.University}";
        }

        var employeeProfile = await employeeRepository.GetByUserIdAsync(guid, ct);
        if (employeeProfile != null)
        {
            authorName = employeeProfile.Name;
            authorRole = employeeProfile.Activity;
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            UserId = guid,
            AuthorName = authorName,
            AuthorRole = authorRole,
            Text = request.Text.Trim(),
            Rating = request.Rating,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };

        await reviewRepository.AddAsync(review, ct);
        logger.LogInformation("Review created by {UserId}", userId);
        return Ok(ToResponse(review));
    }

    [Authorize(Roles = "Admin")]
    [SwaggerOperation("Одобрить отзыв")]
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApproveAsync(Guid id, CancellationToken ct)
    {
        var review = await reviewRepository.GetByIdAsync(id, ct);
        if (review == null)
            return NotFound();

        review.IsApproved = true;
        await reviewRepository.UpdateAsync(review, ct);
        logger.LogInformation("Review {Id} approved", id);
        return Ok(ToResponse(review));
    }

    [Authorize]
    [SwaggerOperation("Удалить отзыв")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var review = await reviewRepository.GetByIdAsync(id, ct);
        if (review == null)
            return NotFound();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && review.UserId != new Guid(userId))
            return Forbid();

        await reviewRepository.DeleteAsync(id, ct);
        logger.LogInformation("Review {Id} deleted by {UserId}", id, userId);
        return Ok();
    }

    private static ReviewResponse ToResponse(Review review) => new()
    {
        Id = review.Id,
        AuthorName = review.AuthorName,
        AuthorRole = review.AuthorRole,
        Text = review.Text,
        Rating = review.Rating,
        IsApproved = review.IsApproved,
        CreatedAt = review.CreatedAt
    };
}
