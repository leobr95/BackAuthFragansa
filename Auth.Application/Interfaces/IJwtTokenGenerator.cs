using Auth.Domain.Entities;

namespace Auth.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string accessToken, DateTime expiresAt) CreateAccessToken(User user);
    (string refreshToken, DateTime expiresAt) CreateRefreshToken();
    string HashRefreshToken(string token); // para persistir hash en DB
}
