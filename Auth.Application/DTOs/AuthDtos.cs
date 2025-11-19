namespace Auth.Application.DTOs;

public record RegisterRequest(string Email, string Password, string FullName);
public record LoginRequest(string Email, string Password);

public record UserDto(Guid UserId, string Email, string FullName, string Role);

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    DateTime RefreshExpiresAt,
    UserDto User
);
