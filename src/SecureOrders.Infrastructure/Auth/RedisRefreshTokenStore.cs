using System.Text.Json;
using StackExchange.Redis;
using SecureOrders.Application.Auth;

namespace SecureOrders.Infrastructure.Auth;

public sealed class RedisRefreshTokenStore : IRefreshTokenStore
{
    private readonly IDatabase _db;

    public RedisRefreshTokenStore(IConnectionMultiplexer mux)
    {
        _db = mux.GetDatabase();
    }

    public Task StoreAsync(string refreshToken, Guid userId, string email, TimeSpan ttl, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (ttl <= TimeSpan.Zero)
            ttl = TimeSpan.FromMinutes(1);

        var key = BuildKey(refreshToken);
        var payload = JsonSerializer.Serialize(new RefreshTokenEntry(userId, email));

        return _db.StringSetAsync(key, payload, ttl);
    }

    public async Task<(bool Found, Guid UserId, string Email)> GetAsync(string refreshToken, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var key = BuildKey(refreshToken);
        var val = await _db.StringGetAsync(key);
        if (!val.HasValue)
            return (false, Guid.Empty, string.Empty);

        var entry = JsonSerializer.Deserialize<RefreshTokenEntry>(val!);
        if (entry is null)
            return (false, Guid.Empty, string.Empty);

        return (true, entry.UserId, entry.Email);
    }

    public Task InvalidateAsync(string refreshToken, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var key = BuildKey(refreshToken);
        return _db.KeyDeleteAsync(key);
    }

    private static string BuildKey(string refreshToken) => $"auth:refresh:{refreshToken}";

    private sealed record RefreshTokenEntry(Guid UserId, string Email);
}
