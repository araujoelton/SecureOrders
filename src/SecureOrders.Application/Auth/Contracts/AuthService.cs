using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SecureOrders.Application.Auth.Contracts;

namespace SecureOrders.Application.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly JwtSettings _jwt;

    // Settings internos no Application (não depende de Api)
    public sealed record JwtSettings(
        string Issuer,
        string Audience,
        string SigningKey,
        int AccessTokenMinutes,
        int RefreshTokenDays);

    public AuthService(IRefreshTokenStore refreshTokenStore, JwtSettings jwt)
    {
        _refreshTokenStore = refreshTokenStore;
        _jwt = jwt;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        // ✅ Fase 1 (demo): usuário fixo
        // Depois a gente troca por validação real (DB/Identity)
        if (request.Username != "admin" || request.Password != "admin")
            throw new UnauthorizedAccessException("Credenciais inválidas.");

        var userId = "1"; // demo
        var access = GenerateAccessToken(userId, request.Username, roles: new[] { "Admin" }, out var expiresAtUtc);
        var refresh = GenerateSecureRefreshToken();

        var refreshExpiresUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
        await _refreshTokenStore.StoreAsync(userId, refresh, refreshExpiresUtc, ct);

        return new AuthResponse(access, refresh, expiresAtUtc);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        var userId = await _refreshTokenStore.GetUserIdByTokenAsync(request.RefreshToken, ct);
        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        // Rotação: revoga o antigo e cria um novo
        await _refreshTokenStore.RevokeAsync(request.RefreshToken, ct);

        var access = GenerateAccessToken(userId, username: "admin", roles: new[] { "Admin" }, out var expiresAtUtc);
        var newRefresh = GenerateSecureRefreshToken();

        var refreshExpiresUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
        await _refreshTokenStore.StoreAsync(userId, newRefresh, refreshExpiresUtc, ct);

        return new AuthResponse(access, newRefresh, expiresAtUtc);
    }

    private string GenerateAccessToken(string userId, string username, IEnumerable<string> roles, out DateTime expiresAtUtc)
    {
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(_jwt.SigningKey);
        var key = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }
}
