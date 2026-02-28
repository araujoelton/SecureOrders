using System.Security.Claims;

namespace SecureOrders.Application.Auth;

public interface IJwtTokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<Claim>? extraClaims = null);
    int GetAccessTokenExpiresInSeconds();
}
