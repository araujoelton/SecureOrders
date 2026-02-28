namespace SecureOrders.Application.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public JwtOptions Jwt { get; init; } = new();
    public int RefreshTokenTtlDays { get; init; } = 7;
    public DemoUserOptions DemoUser { get; init; } = new();
}

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;
    public int ExpiresMinutes { get; init; } = 60;
}

public sealed class DemoUserOptions
{
    public Guid UserId { get; init; } = Guid.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
