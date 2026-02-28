namespace SecureOrders.Application.Auth;

public sealed record AuthResponse(string AccessToken, string RefreshToken, int ExpiresIn);
