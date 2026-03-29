using Trampline.Core.Models;
using Trampline.Shared.Results;

namespace Trampline.Application.Services;

public interface ITokenService
{
    Task<Result<RefreshToken>> ValidateToken(string token, CancellationToken cancellationToken);

    Task<RefreshToken> GenerateToken(User user, UserAgent agent, CancellationToken cancellationToken);

    Task<Result<RefreshToken>> GetSession(string token, CancellationToken cancellationToken);

    Task<Result<List<RefreshToken>>> GetSessions(Guid userId, CancellationToken cancellationToken);

    Task<Result<User>> GetUserFromToken(string token, CancellationToken cancellationToken);

    string GenerateJwtToken(User user);

    Task<Result<User>> GetUserFromJwtToken(string token, CancellationToken cancellationToken);

    string? GetRoleFromJwtToken(string token);

    Task DeleteTokenAsync(string token, CancellationToken cancellationToken);

    Task<IEnumerable<UserSession>> GetTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task DisableTokenAsync(string token, CancellationToken cancellationToken);

    Task BlacklistJwtAsync(string jwtToken, CancellationToken cancellationToken);

    Task<bool> IsJwtBlacklistedAsync(string jti, CancellationToken cancellationToken);
}