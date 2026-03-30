using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Core.Repositories;
using Trampline.Application.Utils;
using Trampline.Web.Controllers.Base;

namespace Trampline.Web.Controllers;

[Route("[controller]")]
[EnableRateLimiting("write")]
public class ReviewController(
    ILogger<ReviewController> logger,
    IReviewRepository reviewRepository,
    IUserService userService,
    IWorkerRepository workerRepository,
    IEmployeeRepository employeeRepository) : BaseApiController
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
        var userGuid = GetUserId();

        if (request.Rating < 1 || request.Rating > 5)
            return BadRequest(new ProblemDetails { Title = "Рейтинг должен быть от 1 до 5", Status = 400 });

        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new ProblemDetails { Title = "Текст отзыва обязателен", Status = 400 });

        var user = await userService.GetByIdAsync(userGuid, ct);
        if (user == null)
            return NotFound();

        var authorName = user.Nickname;
        var authorRole = string.Empty;

        var workerProfile = await workerRepository.GetByUserIdAsync(userGuid, ct);
        if (workerProfile != null)
        {
            authorName = $"{workerProfile.Name} {workerProfile.LastName}";
            if (workerProfile.Info != null)
                authorRole = $"Студент, {workerProfile.Info.University}";
        }

        var employeeProfile = await employeeRepository.GetByUserIdAsync(userGuid, ct);
        if (employeeProfile != null)
        {
            authorName = employeeProfile.Name;
            authorRole = employeeProfile.Activity;
        }

        var review = Review.Create(
            userGuid,
            authorName,
            authorRole,
            HtmlSanitization.Sanitize(request.Text.Trim()),
            request.Rating);

        await reviewRepository.AddAsync(review, ct);
        logger.LogInformation("Review created by {UserId}", userGuid);
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

        review.Approve();
        await reviewRepository.UpdateAsync(review, ct);
        logger.LogInformation("Review {Id} approved", id);
        return Ok(ToResponse(review));
    }

    [Authorize]
    [SwaggerOperation("Удалить отзыв")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        var userGuid = GetUserId();

        var review = await reviewRepository.GetByIdAsync(id, ct);
        if (review == null)
            return NotFound();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && review.UserId != userGuid)
            return Forbid();

        await reviewRepository.DeleteAsync(id, ct);
        logger.LogInformation("Review {Id} deleted by {UserId}", id, userGuid);
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
