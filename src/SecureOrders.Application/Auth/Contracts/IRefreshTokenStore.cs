namespace SecureOrders.Application.Auth;

public interface IRefreshTokenStore
{
    Task StoreAsync(string userId, string refreshToken, DateTime expiresAtUtc, CancellationToken ct);
    Task<string?> GetUserIdByTokenAsync(string refreshToken, CancellationToken ct);
    Task RevokeAsync(string refreshToken, CancellationToken ct);
}
