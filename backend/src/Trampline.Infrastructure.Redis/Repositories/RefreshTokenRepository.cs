using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;

namespace Trampline.Infrastructure.Redis.Repositories;

public class RefreshTokenRepository(ILogger<RefreshTokenRepository> logger, IDistributedCache cache) : IRefreshTokenRepository
{
    private readonly JsonSerializerOptions options = new()
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
    };

    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    private static SemaphoreSlim GetLock(string key) =>
        _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

    public async Task<Result<RefreshToken>> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var findString = await cache.GetStringAsync(token, cancellationToken);
        if (string.IsNullOrWhiteSpace(findString))
            return Result<RefreshToken>.Failure(new ErrorDetail(nameof(token), "Token not found"));
        logger.LogDebug("Retrieved refresh token from cache");
        var findToken = JsonSerializer.Deserialize<RefreshToken>(findString, options)!;

        return Result<RefreshToken>.Success(findToken);
    }

    public async Task<Result<List<RefreshToken>>> GetTokensAsync(string userId, CancellationToken cancellationToken)
    {
        var listKey = $"list:{userId}";
        var listString = await cache.GetStringAsync(listKey, cancellationToken);

        var list = string.IsNullOrEmpty(listString)
            ? new List<RefreshToken>()
            : JsonSerializer.Deserialize<List<RefreshToken>>(listString, options) ?? new List<RefreshToken>();

        return Result<List<RefreshToken>>.Success(list);
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        var tokenString = JsonSerializer.Serialize(token, options);
        logger.LogInformation("Adding refresh token for user {UserId}", token.UserId);
        await cache.SetStringAsync(token.Token, tokenString, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(10)
        }, cancellationToken);

        var listKey = $"list:{token.UserId}";
        var listLock = GetLock(listKey);
        await listLock.WaitAsync(cancellationToken);
        try
        {
            var listString = await cache.GetStringAsync(listKey, cancellationToken);
            var tokenList = string.IsNullOrEmpty(listString)
                ? new List<RefreshToken>()
                : JsonSerializer.Deserialize<List<RefreshToken>>(listString, options) ?? new List<RefreshToken>();

            if (!tokenList.Any(t => t.Token == token.Token))
            {
                tokenList.Add(token);
                var updatedListJson = JsonSerializer.Serialize(tokenList, options);
                await cache.SetStringAsync(listKey, updatedListJson, cancellationToken);
            }
        }
        finally
        {
            listLock.Release();
        }
    }

    public async Task UpdateAsync(RefreshToken token, CancellationToken cancellationToken)
    {
        var tokenString = JsonSerializer.Serialize(token, options);
        logger.LogInformation("Updating refresh token for user {UserId}", token.UserId);
        await cache.SetStringAsync(token.Token, tokenString, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(10)
        }, cancellationToken);

        var listKey = $"list:{token.UserId}";
        var listLock = GetLock(listKey);
        await listLock.WaitAsync(cancellationToken);
        try
        {
            var listString = await cache.GetStringAsync(listKey, cancellationToken);
            var tokenList = string.IsNullOrEmpty(listString)
                ? new List<RefreshToken>()
                : JsonSerializer.Deserialize<List<RefreshToken>>(listString, options) ?? new List<RefreshToken>();

            var index = tokenList.FindIndex(t => t.Token == token.Token);

            if (index >= 0)
                tokenList[index] = token;
            else
                tokenList.Add(token);

            var updatedListJson = JsonSerializer.Serialize(tokenList, options);
            await cache.SetStringAsync(listKey, updatedListJson, cancellationToken);
        }
        finally
        {
            listLock.Release();
        }
    }

    public async Task DeleteAsync(string token, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleted refresh token");
        await cache.RemoveAsync(token, cancellationToken);
    }

    public async Task DisableTokenAsync(string token, CancellationToken cancellationToken)
    {
        var find = await GetByTokenAsync(token, cancellationToken);

        if (find.IsSuccess)
        {
            find.Value!.Revoked = DateTime.UtcNow;
            await UpdateAsync(find.Value!, cancellationToken);
        }
    }

    public async Task DisableAllRefreshAsync(List<RefreshToken> refreshTokens, CancellationToken cancellationToken, string? refreshToken = null)
    {
        foreach (var item in refreshTokens)
        {
            if (refreshToken != null && item.Token == refreshToken)
                continue;

            item.Revoked = DateTime.UtcNow;
            await UpdateAsync(item, cancellationToken);
        }
    }
}
