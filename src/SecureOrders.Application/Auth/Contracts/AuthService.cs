using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace SecureOrders.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly AuthOptions _options;

    public AuthService(
        IRefreshTokenStore refreshTokenStore,
        IJwtTokenService jwtTokenService,
        AuthOptions options)
    {
        _refreshTokenStore = refreshTokenStore;
        _jwtTokenService = jwtTokenService;
        _options = options;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new ArgumentException("Payload inválido.");

        if (!IsDemoUserValid(request.Email, request.Password))
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var accessToken = _jwtTokenService.GenerateAccessToken(
            _options.DemoUser.UserId,
            _options.DemoUser.Email);

        var refreshToken = GenerateSecureRefreshToken();
        var ttl = TimeSpan.FromDays(_options.RefreshTokenTtlDays);

        await _refreshTokenStore.StoreAsync(
            refreshToken,
            _options.DemoUser.UserId,
            _options.DemoUser.Email,
            ttl,
            ct);

        return new AuthResponse(
            accessToken,
            refreshToken,
            _jwtTokenService.GetAccessTokenExpiresInSeconds());
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Payload inválido.");

        var entry = await _refreshTokenStore.GetAsync(request.RefreshToken, ct);
        if (!entry.Found)
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        await _refreshTokenStore.InvalidateAsync(request.RefreshToken, ct);

        var accessToken = _jwtTokenService.GenerateAccessToken(entry.UserId, entry.Email);
        var newRefreshToken = GenerateSecureRefreshToken();
        var ttl = TimeSpan.FromDays(_options.RefreshTokenTtlDays);

        await _refreshTokenStore.StoreAsync(
            newRefreshToken,
            entry.UserId,
            entry.Email,
            ttl,
            ct);

        return new AuthResponse(
            accessToken,
            newRefreshToken,
            _jwtTokenService.GetAccessTokenExpiresInSeconds());
    }

    private bool IsDemoUserValid(string email, string password)
    {
        return string.Equals(email, _options.DemoUser.Email, StringComparison.OrdinalIgnoreCase)
               && string.Equals(password, _options.DemoUser.Password, StringComparison.Ordinal);
    }

    private static string GenerateSecureRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Base64UrlEncoder.Encode(bytes);
    }
}
