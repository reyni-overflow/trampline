using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Trampline.Web.Controllers.Base;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected Guid GetUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return id is null ? throw new UnauthorizedAccessException() : Guid.Parse(id);
    }

    protected string? GetUserIdString() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    protected string GetRole() => User.FindFirstValue(ClaimTypes.Role) ?? "Worker";
}
