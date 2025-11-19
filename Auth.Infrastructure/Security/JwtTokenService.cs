using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure.Security;

public class JwtTokenService : IJwtTokenGenerator
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly TimeSpan _accessLifetime;
    private readonly TimeSpan _refreshLifetime;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenService(IConfiguration cfg)
    {
        _issuer = cfg["Jwt:Issuer"] ?? "auth";
        _audience = cfg["Jwt:Audience"] ?? "debts";
        _accessLifetime = TimeSpan.FromMinutes(int.TryParse(cfg["Jwt:AccessTokenMinutes"], out var m) ? m : 60);
        _refreshLifetime = TimeSpan.FromDays(int.TryParse(cfg["Jwt:RefreshTokenDays"], out var d) ? d : 7);
        var secret = cfg["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret missing");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    public (string accessToken, DateTime expiresAt) CreateAccessToken(User user)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var exp = now.Add(_accessLifetime);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, ((DateTimeOffset)now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: now,
            expires: exp,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), exp);
    }

    public (string refreshToken, DateTime expiresAt) CreateRefreshToken()
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return (token, DateTime.UtcNow.Add(_refreshLifetime));
    }

    public string HashRefreshToken(string token)
        => BCrypt.Net.BCrypt.HashPassword(token);
}
