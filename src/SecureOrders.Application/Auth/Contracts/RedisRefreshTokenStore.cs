using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;
using SecureOrders.Application.Auth;

namespace SecureOrders.Infrastructure.Auth;

public sealed class RedisRefreshTokenStore : IRefreshTokenStore
{
    private readonly StackExchange.Redis.IDatabase _db;

    // Key: refresh:{token} -> userId
    // TTL: expiração do refresh
    public RedisRefreshTokenStore(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }

    public Task StoreAsync(string userId, string refreshToken, DateTime expiresAtUtc, CancellationToken ct)
    {
        var key = BuildKey(refreshToken);
        var ttl = expiresAtUtc - DateTime.UtcNow;
        if (ttl <= TimeSpan.Zero) ttl = TimeSpan.FromMinutes(1);

        return _db.StringSetAsync(key, userId, ttl);
    }

    public async Task<string?> GetUserIdByTokenAsync(string refreshToken, CancellationToken ct)
    {
        var key = BuildKey(refreshToken);
        var val = await _db.StringGetAsync(key);
        return val.HasValue ? val.ToString() : null;
    }

    public Task RevokeAsync(string refreshToken, CancellationToken ct)
    {
        var key = BuildKey(refreshToken);
        return _db.KeyDeleteAsync(key);
    }

    private static string BuildKey(string refreshToken) => $"refresh:{refreshToken}";
}
