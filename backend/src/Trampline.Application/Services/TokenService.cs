using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Trampline.Core.Models;
using Trampline.Core.Options;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;
using Trampline.Shared.Services;

namespace Trampline.Application.Services;

public class TokenService(
    ILogger<TokenService> logger,
    IRefreshTokenRepository refreshTokenRepository,
    IUserService userService,
    IOptions<JwtOption> jwtOption,
    IInfoService infoService,
    IUserSessionRepository userSessionRepository,
    IDistributedCache cache) : ITokenService
{
    private readonly JwtOption jwtOpt = jwtOption.Value;

    public async Task<Result<RefreshToken>> ValidateToken(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("Invalid refresh token attempted");
            return Result<RefreshToken>.Failure(new ErrorDetail(nameof(token), "refresh token is null"));
        }

        var findToken = await refreshTokenRepository.GetByTokenAsync(token, cancellationToken);

        if (findToken.IsFailure)
        {
            logger.LogWarning("Invalid refresh token attempted");
            return Result<RefreshToken>.Failure(findToken.Errors.ToArray());
        }

        if (!findToken.Value!.IsActive)
        {
            logger.LogWarning("Revoked or expired refresh token attempted");
            if (findToken.Value.Revoked == null)
            {
                findToken.Value.Revoked = DateTime.UtcNow;
                await refreshTokenRepository.UpdateAsync(findToken.Value, cancellationToken);
            }
            return Result<RefreshToken>.Failure(new ErrorDetail(nameof(token),
                "RefreshToken is no longer active"));
        }

        return Result<RefreshToken>.Success(findToken.Value);
    }

    public async Task<RefreshToken> GenerateToken(User user, UserAgent agent, CancellationToken cancellationToken)
    {
        const int byteLength = 48;
        byte[] randomBytes = new byte[byteLength];

        using (var rng = RandomNumberGenerator.Create())
            rng.GetBytes(randomBytes);

        string base64Token = Convert.ToBase64String(randomBytes);
        base64Token = "sk-" + base64Token.TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var location = agent.Ip != "Unknown device"
            ? await infoService.GetLocation(agent.Ip, cancellationToken)
            : agent.Ip;

        RefreshToken token = new()
        {
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            Id = Guid.NewGuid(),
            Token = PasswordHasher.HashToken(base64Token),
            UserId = user.Id,
            Agent = agent.Agent,
            Location = location,
        };
        await refreshTokenRepository.AddAsync(token, cancellationToken);
        var session = UserSession.Create(user.Id, token.Token, agent);
        await userSessionRepository.AddAsync(session, cancellationToken);
        user.AddSession(session);
        await userService.UpdateAsync(user, cancellationToken);

        logger.LogDebug("Refresh token generated for {UserId}, device: {DeviceName}", user.Id, agent.Agent);
        return token;
    }

    public async Task<Result<RefreshToken>> GetSession(string token, CancellationToken cancellationToken)
    {
        var find = await refreshTokenRepository.GetByTokenAsync(token, cancellationToken);

        return find;
    }

    public async Task<Result<List<RefreshToken>>> GetSessions(Guid userId, CancellationToken cancellationToken)
    {
        var result = await refreshTokenRepository.GetTokensAsync(userId.ToString(), cancellationToken);

        return result;
    }

    public async Task<Result<User>> GetUserFromToken(string token, CancellationToken cancellationToken)
    {
        var findToken = await refreshTokenRepository.GetByTokenAsync(token, cancellationToken);

        if (findToken.IsFailure)
        {
            return Result<User>.Failure(findToken.Errors.ToArray());
        }

        var findUser = await userService.GetByIdAsync(findToken.Value!.UserId, cancellationToken);

        if (findUser == null)
        {
            return Result<User>.Failure(new ErrorDetail(nameof(token), "User not found", 404));
        }

        return Result<User>.Success(findUser);
    }

    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),

            new(JwtRegisteredClaimNames.Profile, user.Role.ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),

            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Email, user.Email),

            new(JwtRegisteredClaimNames.Picture, user.Avatar ?? ""),
            new("avatar", user.Avatar ?? ""),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: jwtOpt.Issuer,
            audience: jwtOpt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(30)),
            signingCredentials: creds);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        logger.LogDebug("JWT generated for {UserId}", user.Id);
        return token;
    }

    public async Task<Result<User>> GetUserFromJwtToken(string token, CancellationToken cancellationToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var id = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Sub)?.Value;

        if (id == null)
            return Result<User>.Failure(new ErrorDetail(nameof(token), "User not found"));

        var findUser = await userService.GetByIdAsync(new Guid(id), cancellationToken);

        if (findUser == null)
            return Result<User>.Failure(new ErrorDetail("user", "User not found", 404));
        return Result<User>.Success(findUser);
    }

    public string? GetRoleFromJwtToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
    }

    public async Task DeleteTokenAsync(string token, CancellationToken cancellationToken)
    {
        var findToken = await refreshTokenRepository.GetByTokenAsync(token, cancellationToken);

        if (findToken.IsFailure)
            return;

        await refreshTokenRepository.DeleteAsync(findToken.Value!.Token, cancellationToken);
        logger.LogDebug("Token deleted");
    }

    public async Task<IEnumerable<UserSession>> GetTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userService.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return Enumerable.Empty<UserSession>();
        }

        return user.Sessions.Where(x => x.IsActive);
    }

    public async Task DisableTokenAsync(string token, CancellationToken cancellationToken)
    {
        await refreshTokenRepository.DisableTokenAsync(token, cancellationToken);
        logger.LogDebug("Token disabled for user");
    }

    public async Task BlacklistJwtAsync(string jwtToken, CancellationToken cancellationToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(jwtToken);
            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
                return;

            var remaining = jwt.ValidTo - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
                return;

            await cache.SetStringAsync(
                $"jwt_blacklist:{jti}",
                "1",
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = remaining },
                cancellationToken);

            logger.LogDebug("JWT blacklisted: {Jti}", jti);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to blacklist JWT");
        }
    }

    public async Task<bool> IsJwtBlacklistedAsync(string jti, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(jti))
            return false;

        var value = await cache.GetStringAsync($"jwt_blacklist:{jti}", cancellationToken);
        return value != null;
    }
}