using System.Security.Claims;

namespace JwtAuthLibrary.Services
{
    public interface IJwtAuthService
    {
        string CreateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
