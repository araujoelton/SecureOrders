namespace SecureOrders.Application.Auth.Contracts;

public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAtUtc);
