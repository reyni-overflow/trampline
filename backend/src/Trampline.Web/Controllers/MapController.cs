using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Core.Repositories;
using Trampline.Web.Controllers.Base;

namespace Trampline.Web.Controllers;

[Route("[controller]")]
[EnableRateLimiting("api")]
public class MapController(IMapRepository mapRepository) : BaseApiController
{
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Маркеры для карты", Description = "Лёгкий endpoint — только поля для маркеров")]
    [HttpGet("markers")]
    public async Task<IActionResult> GetMarkersAsync(CancellationToken ct)
    {
        var markers = await mapRepository.GetMarkersAsync(ct);
        return Ok(markers);
    }
}
