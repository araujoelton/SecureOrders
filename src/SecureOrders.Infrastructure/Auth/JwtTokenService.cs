using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SecureOrders.Application.Auth;

namespace SecureOrders.Infrastructure.Auth;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly AuthOptions _options;

    public JwtTokenService(AuthOptions options)
    {
        _options = options;
    }

    public string GenerateAccessToken(Guid userId, string email, IEnumerable<Claim>? extraClaims = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        if (extraClaims is not null)
            claims.AddRange(extraClaims);

        var expires = DateTime.UtcNow.AddMinutes(_options.Jwt.ExpiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Jwt.Issuer,
            audience: _options.Jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int GetAccessTokenExpiresInSeconds()
    {
        return (int)TimeSpan.FromMinutes(_options.Jwt.ExpiresMinutes).TotalSeconds;
    }
}
