using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;

public interface IAuthService
{
    Task<(bool ok, AuthResponse? result, string[] errors)> RegisterAsync(RegisterRequest req, CancellationToken ct = default);
    Task<(bool ok, AuthResponse? result, string[] errors)> LoginAsync(LoginRequest req, CancellationToken ct = default);
    //Task<(bool ok, AuthResponse? result, string[] errors)> RefreshAsync(Guid userId, string refreshToken, CancellationToken ct = default);
    Task<UserDto?> GetMeAsync(Guid userId, CancellationToken ct = default);
}
