using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Distributed;
using User = Trampline.Core.Models.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Trampline.Application.Services;
using Trampline.Application.Services.IO;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Web.Extensions;

namespace Trampline.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(
    ILogger<AuthController> logger,
    IAuthService authService,
    IUserService userService,
    ITokenService tokenService,
    IRefreshTokenRepository tokenRepository,
    IUserSessionRepository userSessionRepository,
    IMediaService mediaService,
    ITotpService totpService,
    IDistributedCache cache,
    IWebHostEnvironment environment) : ControllerBase
{
    private CookieOptions CreateCookieOptions(TimeSpan expiry)
    {
        var isHttps = HttpContext.Request.IsHttps;
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = isHttps,
            SameSite = isHttps ? SameSiteMode.Strict : SameSiteMode.Lax,
            Path = "/",
            Expires = DateTime.UtcNow.Add(expiry)
        };
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var userAgent = HttpContext.Request.Headers.UserAgent;
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "Unknown device";

        var result = await authService.RegisterAsync(request, new UserAgent(ip, userAgent.ToString()), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        Response.Cookies.Append("jwt_token", result.Value!.AccessToken, CreateCookieOptions(TimeSpan.FromMinutes(30)));
        Response.Cookies.Append("refresh_token", result.Value!.RefreshToken, CreateCookieOptions(TimeSpan.FromDays(7)));

        logger.LogInformation("User registered from {IP}", ip);
        return Ok(new { id = result.Value!.Id });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var userAgent = HttpContext.Request.Headers.UserAgent;
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "Unknown device";

        var result = await authService.LoginAsync(request, new UserAgent(ip, userAgent.ToString()), cancellationToken);

        if (result.IsFailure)
        {
            logger.LogWarning("Login failed from {IP}", ip);
            return result.ToActionResult();
        }

        var loginUser = await userService.GetByIdAsync(result.Value!.Id, cancellationToken);
        if (loginUser?.IsTotpEnabled == true)
        {
            var challengeId = Guid.NewGuid().ToString("N");
            var pendingKey = $"totp_pending:{challengeId}";
            await cache.SetStringAsync(pendingKey,
                $"{loginUser.Id}|{result.Value!.AccessToken}|{result.Value!.RefreshToken}",
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) },
                cancellationToken);
            return Ok(new { requiresTotp = true, challengeId });
        }

        Response.Cookies.Append("jwt_token", result.Value!.AccessToken, CreateCookieOptions(TimeSpan.FromMinutes(30)));
        Response.Cookies.Append("refresh_token", result.Value!.RefreshToken, CreateCookieOptions(TimeSpan.FromDays(7)));

        logger.LogInformation("User logged in from {IP}", ip);
        return Ok(new { id = result.Value!.Id });
    }

    [EnableRateLimiting("auth")]
    [HttpGet("refresh")]
    public async Task<IActionResult> RefreshAsync(CancellationToken cancellationToken)
    {
        HttpContext.Request.Cookies.TryGetValue("refresh_token", out var token);

        var userAgent = HttpContext.Request.Headers.UserAgent;
        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "Unknown device";

        var result = await authService.RefreshAsync(token ?? "", new UserAgent(ip, userAgent.ToString()), cancellationToken);

        if (result.IsFailure)
        {
            return result.ToActionResult();
        }

        Response.Cookies.Append("jwt_token", result.Value!.AccessToken, CreateCookieOptions(TimeSpan.FromMinutes(30)));
        Response.Cookies.Append("refresh_token", result.Value!.RefreshToken, CreateCookieOptions(TimeSpan.FromDays(7)));

        logger.LogDebug("Token refreshed");
        return Ok(new { id = result.Value!.Id });
    }

    [SwaggerResponse(200, "Чистит куки")]
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        if (HttpContext.Request.Cookies.TryGetValue("refresh_token", out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
        {
            await tokenService.DisableTokenAsync(refreshToken, cancellationToken);
        }

        if (HttpContext.Request.Cookies.TryGetValue("jwt_token", out var jwtToken) && !string.IsNullOrEmpty(jwtToken))
        {
            await tokenService.BlacklistJwtAsync(jwtToken, cancellationToken);
        }

        Response.Cookies.Delete("jwt_token", CreateCookieOptions(TimeSpan.Zero));
        Response.Cookies.Delete("refresh_token", CreateCookieOptions(TimeSpan.Zero));

        logger.LogInformation("User logged out");
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMeAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var roleLine = User.FindFirst(ClaimTypes.Role)?.Value
                       ?? User.FindFirst(JwtRegisteredClaimNames.Profile)?.Value;
        var role = Enum.Parse<Role>(roleLine ?? "Worker", ignoreCase: true);

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new ProblemDetails()
            {
                Title = "token is invalid",
                Status = 400,
                Detail = "Please provide a valid token"
            });
        }

        UserResponse? response = null;

        if (role == Role.Employee)
        {
            response = await userService.GetMeAsync(new Guid(userId), user => new UserResponse()
            {
                Id = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Role = user.Role,
                IsTotpEnabled = user.IsTotpEnabled,
                MustChangePassword = user.MustChangePassword,
                EmployeeProfile = user.EmployeeProfile != null
                    ? new EmployeeProfileResponse
                    {
                        Id = user.EmployeeProfile.Id,
                        UserId = user.Id,
                        Activity = user.EmployeeProfile.Activity,
                        Description = user.EmployeeProfile.Description,
                        Name = user.EmployeeProfile.Name,
                        Photos = user.EmployeeProfile.Photos,
                        IsVerified = user.EmployeeProfile.IsVerified,
                        VerificationLevel = user.EmployeeProfile.VerificationLevel,
                        VerifiedName = user.EmployeeProfile.VerifiedName,
                        Link = user.EmployeeProfile.Link,
                        Socials = user.EmployeeProfile.Socials,
                        Videos = user.EmployeeProfile.Videos,
                        Info = user.EmployeeProfile.Info
                    }
                    : null
            }, cancellationToken);
        }
        else if (role == Role.Worker)
        {
            response = await userService.GetMeAsync(new Guid(userId), user => new UserResponse()
            {
                Id = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Role = user.Role,
                IsTotpEnabled = user.IsTotpEnabled,
                MustChangePassword = user.MustChangePassword,
                WorkerProfile = user.WorkerProfile != null
                    ? new WorkerProfileResponse()
                    {
                        Name = user.WorkerProfile.Name,
                        LastName = user.WorkerProfile.LastName,
                        About = user.WorkerProfile.About,
                        Patronymic = user.WorkerProfile.Patronymic,
                        Photo = user.WorkerProfile.Photo,
                        Resume = user.WorkerProfile.Resume,
                        Info = user.WorkerProfile.Info,
                        Skills = user.WorkerProfile.Skills,
                        Repos = user.WorkerProfile.Repos
                    }
                    : null,
            }, cancellationToken);
        }
        else
        {
            response = await userService.GetMeAsync(new Guid(userId), user => new UserResponse()
            {
                Id = user.Id,
                Email = user.Email,
                Nickname = user.Nickname,
                Avatar = user.Avatar,
                Role = user.Role,
                IsTotpEnabled = user.IsTotpEnabled,
                MustChangePassword = user.MustChangePassword,
                IsSuperAdmin = user.IsSuperAdmin,
            }, cancellationToken);
        }

        if (response == null)
        {
            return NotFound(new ProblemDetails()
            {
                Title = "user not found",
                Status = 404,
                Detail = "user not found"
            });
        }

        return Ok(response);
    }

    [SwaggerOperation(Summary = "Получение текущий сессии")]
    [Authorize]
    [HttpGet("sessions/{token}")]
    public async Task<IActionResult> GetSessions(string token, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await tokenService.GetSession(token, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        if (result.Value!.UserId != new Guid(userId))
            return Forbid();

        return result.ToActionResult();
    }

    [SwaggerOperation(Summary = "Получение сессий")]
    [Authorize]
    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions(CancellationToken cancellationToken)
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

        var id = new Guid(userId);

        var sessions = await tokenService.GetTokensByUserIdAsync(id, cancellationToken);
        return Ok(sessions.Select(s => new
        {
            s.Id,
            s.UserId,
            s.DeviceName,
            UserAgent = new { Ip = s.UserAgent.Ip, Agent = s.UserAgent.Agent },
            s.CreatedAt,
            s.LastUsedAt,
            s.ExpiresAt,
            s.RevokedAt,
            s.IsActive
        }));
    }

    [SwaggerOperation(Summary = "Заркыть все сессии кроме текущий")]
    [Authorize]
    [HttpPost("sessions")]
    public async Task<IActionResult> DisableSessions(CancellationToken cancellationToken)
    {
        HttpContext.Request.Cookies.TryGetValue("refresh_token", out var refreshToken);
        if (refreshToken == null)
            return Unauthorized("refresh токен не был найден");

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

        var id = new Guid(userId);

        var tokens = await tokenService.GetSessions(id, cancellationToken);
        if (tokens.Value != null)
            await tokenRepository.DisableAllRefreshAsync(tokens.Value, cancellationToken, refreshToken);

        logger.LogInformation("All sessions closed for user");
        return Ok("Все сессии закрыты");
    }

    [SwaggerOperation(Summary = "Закрыть сессию по ID")]
    [Authorize]
    [HttpDelete("sessions/{sessionId:guid}")]
    public async Task<IActionResult> DisableSessionById(Guid sessionId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var session = await userSessionRepository.GetByIdAsync(sessionId, cancellationToken);

        if (session == null)
            return NotFound("Сессия не найдена");

        if (session.UserId != new Guid(userId))
            return Forbid();

        session.Revoke("Terminated by user");
        await userSessionRepository.UpdateAsync(session, cancellationToken);

        logger.LogInformation("Session {SessionId} closed by user", sessionId);
        return Ok();
    }

    [SwaggerOperation(Summary = "Смена пароля")]
    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var result = await authService.ChangePasswordAsync(new Guid(userId), request.CurrentPassword, request.NewPassword, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        Response.Cookies.Delete("jwt_token", CreateCookieOptions(TimeSpan.Zero));
        Response.Cookies.Delete("refresh_token", CreateCookieOptions(TimeSpan.Zero));

        logger.LogInformation("Password changed");
        return Ok();
    }

    [SwaggerOperation(Summary = "Удаление аккаунта")]
    [Authorize]
    [HttpPost("delete-account")]
    public async Task<IActionResult> DeleteAccountAsync(DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var result = await authService.DeleteAccountAsync(new Guid(userId), request.Password, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        Response.Cookies.Delete("jwt_token", CreateCookieOptions(TimeSpan.Zero));
        Response.Cookies.Delete("refresh_token", CreateCookieOptions(TimeSpan.Zero));

        logger.LogInformation("Account deleted");
        return Ok();
    }

    [SwaggerOperation(Summary = "Обновить настройки приватности")]
    [Authorize]
    [HttpPut("privacy")]
    public async Task<IActionResult> UpdatePrivacyAsync(UpdatePrivacyRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var user = await userService.GetByIdAsync(new Guid(userId), cancellationToken);
        if (user == null) return NotFound();

        user.SetPrivate(request.IsPrivate);
        await userService.UpdateAsync(user, cancellationToken);

        return Ok();
    }

    [SwaggerOperation(Summary = "Загрузка аватарки пользователя")]
    [Authorize]
    [EnableRateLimiting("upload")]
    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatarAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new ProblemDetails { Title = "token is invalid", Status = 400 });

        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
            return BadRequest(new ProblemDetails { Title = "Invalid file type", Status = 400, Detail = "Only .jpg, .png, .webp are allowed" });

        var result = await mediaService.UploadFile(file, cancellationToken);
        if (result.IsFailure) return result.ToActionResult();

        var user = await userService.GetByIdAsync(new Guid(userId), cancellationToken);
        if (user == null) return NotFound();

        user.SetAvatar(result.Value!);
        await userService.UpdateAsync(user, cancellationToken);

        return Ok(result.Value!);
    }

    public record TotpVerifyRequest(string ChallengeId, string Code);
    public record TotpCodeRequest(string Code);

    [Authorize]
    [HttpPost("totp/setup")]
    public async Task<IActionResult> TotpSetupAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await userService.GetByIdAsync(new Guid(userId), cancellationToken);
        if (user == null) return NotFound();

        if (user.IsTotpEnabled)
            return BadRequest(new ProblemDetails { Title = "TOTP already enabled", Status = 400 });

        var (secret, uri) = totpService.GenerateSetup(user.Email);
        user.SetTotpSecret(secret);
        await userService.UpdateAsync(user, cancellationToken);

        return Ok(new { secret, uri });
    }

    [Authorize]
    [HttpPost("totp/enable")]
    public async Task<IActionResult> TotpEnableAsync([FromBody] TotpCodeRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await userService.GetByIdAsync(new Guid(userId), cancellationToken);
        if (user == null) return NotFound();
        if (string.IsNullOrEmpty(user.TotpSecret))
            return BadRequest(new ProblemDetails { Title = "Call /auth/totp/setup first", Status = 400 });

        if (!totpService.Verify(user.TotpSecret, request.Code))
            return BadRequest(new ProblemDetails { Title = "Invalid TOTP code", Status = 400 });

        user.EnableTotp(user.TotpSecret);
        await userService.UpdateAsync(user, cancellationToken);

        logger.LogInformation("TOTP enabled for user {UserId}", userId);
        return Ok();
    }

    [Authorize]
    [HttpPost("totp/disable")]
    public async Task<IActionResult> TotpDisableAsync([FromBody] TotpCodeRequest request, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await userService.GetByIdAsync(new Guid(userId), cancellationToken);
        if (user == null) return NotFound();
        if (!user.IsTotpEnabled)
            return BadRequest(new ProblemDetails { Title = "TOTP not enabled", Status = 400 });

        if (!totpService.Verify(user.TotpSecret!, request.Code))
            return BadRequest(new ProblemDetails { Title = "Invalid TOTP code", Status = 400 });

        user.DisableTotp();
        await userService.UpdateAsync(user, cancellationToken);

        logger.LogInformation("TOTP disabled for user {UserId}", userId);
        return Ok();
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("totp/verify")]
    public async Task<IActionResult> TotpVerifyAsync([FromBody] TotpVerifyRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ChallengeId))
            return BadRequest(new ProblemDetails { Title = "No pending TOTP verification", Status = 400 });

        var pendingKey = $"totp_pending:{request.ChallengeId}";
        var pendingData = await cache.GetStringAsync(pendingKey, cancellationToken);
        if (string.IsNullOrEmpty(pendingData))
            return BadRequest(new ProblemDetails { Title = "No pending TOTP verification", Status = 400 });

        var parts = pendingData.Split('|', 3);
        if (parts.Length < 3 || !Guid.TryParse(parts[0], out var userId))
            return BadRequest(new ProblemDetails { Title = "No pending TOTP verification", Status = 400 });

        var user = await userService.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsTotpEnabled || string.IsNullOrEmpty(user.TotpSecret))
            return BadRequest(new ProblemDetails { Title = "TOTP not configured", Status = 400 });

        if (!totpService.Verify(user.TotpSecret, request.Code))
            return BadRequest(new ProblemDetails { Title = "Invalid TOTP code", Status = 400 });

        Response.Cookies.Append("jwt_token", parts[1], CreateCookieOptions(TimeSpan.FromMinutes(30)));
        Response.Cookies.Append("refresh_token", parts[2], CreateCookieOptions(TimeSpan.FromDays(7)));

        await cache.RemoveAsync(pendingKey, cancellationToken);

        logger.LogInformation("TOTP verified for user {UserId}", userId);
        return Ok(new { id = userId });
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ForgotPasswordAsync(request.Email, cancellationToken);

        logger.LogInformation("Password reset requested");

        var code = result.Value;
        if (environment.IsDevelopment() && code != null && code.Length == 6 && code.All(char.IsDigit))
            return Ok(new { message = "If the email exists, a reset code has been sent", debugCode = code });

        return Ok(new { message = "If the email exists, a reset code has been sent" });
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword, cancellationToken);

        if (result.IsFailure)
            return result.ToActionResult();

        logger.LogInformation("Password reset completed");
        return Ok(new { message = "Password has been reset successfully" });
    }
}