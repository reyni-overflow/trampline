using Trampline.Core.Models;
using Trampline.Shared.Results;

namespace Trampline.Core.Repositories;

public interface IRefreshTokenRepository
{
    Task<Result<RefreshToken>> GetByTokenAsync(string token, CancellationToken cancellationToken);

    Task<Result<List<RefreshToken>>> GetTokensAsync(string userId, CancellationToken cancellationToken);

    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);

    Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken);

    Task DeleteAsync(string token, CancellationToken cancellationToken);

    Task DisableTokenAsync(string token, CancellationToken cancellationToken);

    Task DisableAllRefreshAsync(List<RefreshToken> refreshTokens, CancellationToken cancellationToken,
        string? refreshToken = null);
}