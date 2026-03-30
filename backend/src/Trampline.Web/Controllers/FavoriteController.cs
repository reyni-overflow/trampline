using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Microsoft.AspNetCore.RateLimiting;
using Trampline.Core.Repositories;
using Trampline.Web.Controllers.Base;

namespace Trampline.Web.Controllers;

[Authorize]
[Route("[controller]")]
[EnableRateLimiting("api")]
public class FavoriteController(
    ILogger<FavoriteController> logger,
    IFavoriteRepository favoriteRepository,
    IJobRepository jobRepository,
    IEventRepository eventRepository,
    IMentorshipRepository mentorshipRepository,
    IEmployeeRepository employeeRepository) : BaseApiController
{
    [SwaggerOperation("Получить избранное")]
    [HttpGet]
    public async Task<IActionResult> GetFavoritesAsync(CancellationToken ct)
    {
        var userGuid = GetUserId();

        var favorites = await favoriteRepository.GetByUserAsync(userGuid, ct);
        return Ok(favorites.Select(f => new
        {
            f.Id,
            f.TargetId,
            type = f.Type.ToString(),
            f.CreatedAt
        }));
    }

    [SwaggerOperation("Добавить/удалить из избранного (toggle)")]
    [HttpPost]
    public async Task<IActionResult> ToggleFavoriteAsync([FromBody] ToggleFavoriteRequest request, CancellationToken ct)
    {
        var userGuid = GetUserId();

        if (!Enum.TryParse<FavoriteType>(request.Type, true, out var type))
            return BadRequest(new ProblemDetails { Title = "Invalid favorite type. Allowed: Job, Company, Event, Mentorship", Status = 400 });

        var existing = await favoriteRepository.FindAsync(userGuid, request.TargetId, type, ct);

        if (existing != null)
        {
            await favoriteRepository.DeleteAsync(existing.Id, ct);
            logger.LogDebug("Favorite toggled: {TargetId} type {Type} by {UserId}", request.TargetId, type, userGuid);
            return Ok(new { added = false });
        }

        var targetExists = type switch
        {
            FavoriteType.Job => await jobRepository.GetByIdAsync(request.TargetId, ct) != null,
            FavoriteType.Event => await eventRepository.GetByIdAsync(request.TargetId, ct) != null,
            FavoriteType.Mentorship => await mentorshipRepository.GetByIdAsync(request.TargetId, ct) != null,
            FavoriteType.Company => await employeeRepository.GetByIdAsync(request.TargetId, ct) != null,
            _ => false
        };

        if (!targetExists)
            return NotFound(new ProblemDetails { Title = "Target entity not found", Status = 404 });

        var fav = Favorite.Create(userGuid, request.TargetId, type);
        await favoriteRepository.AddAsync(fav, ct);
        logger.LogDebug("Favorite toggled: {TargetId} type {Type} by {UserId}", request.TargetId, type, userGuid);
        return Ok(new { added = true, id = fav.Id });
    }
}
