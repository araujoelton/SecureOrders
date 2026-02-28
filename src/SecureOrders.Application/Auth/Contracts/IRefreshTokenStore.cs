namespace SecureOrders.Application.Auth;

public interface IRefreshTokenStore
{
    Task StoreAsync(string refreshToken, Guid userId, string email, TimeSpan ttl, CancellationToken ct);
    Task<(bool Found, Guid UserId, string Email)> GetAsync(string refreshToken, CancellationToken ct);
    Task InvalidateAsync(string refreshToken, CancellationToken ct);
}
