using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FloraAI.API.Models.Entities;
using FloraAI.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FloraAI.API.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var keyStr = _config["Jwt:Key"] ?? _config["JWT_KEY"] ?? "Fallback_Security_Key_For_Development_Only_Change_Immediately";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var validityMinsStr = _config["Jwt:TokenValidityInMinutes"] ?? _config["JWT_TOKEN_VALIDITY_MINUTES"] ?? "60";
        if (!double.TryParse(validityMinsStr, out var validityMins)) validityMins = 60;

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "FloraAI_Backend",
            audience: _config["Jwt:Audience"] ?? "FloraAI_Mobile_App",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(validityMins),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var keyStr = _config["Jwt:Key"] ?? _config["JWT_KEY"] ?? "Fallback_Security_Key_For_Development_Only_Change_Immediately";
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr)),
            ValidateLifetime = false // Here we are saying that we don't care about the token's expiration date
        };


        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken || 
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
