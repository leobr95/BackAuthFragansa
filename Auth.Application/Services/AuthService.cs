using System.Linq;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;

namespace Auth.Application.Services;

public class AuthService(IUserRepository repo, IPasswordHasher hasher, IJwtTokenGenerator jwt) : IAuthService
{
    public async Task<(bool ok, AuthResponse? result, string[] errors)> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var errors = ValidateRegister(req);
        if (errors.Count > 0) return (false, null, errors.ToArray());

        var email = req.Email.Trim().ToLowerInvariant();
        if (await repo.EmailExistsAsync(email, ct))
            return (false, null, new[] { "Email is already registered." });

        var user = new User
        {
            Email = email,
            PasswordHash = hasher.Hash(req.Password),
            FullName = req.FullName.Trim(),
            Role = "user"
        };

        var id = await repo.CreateAsync(user, ct);
        user.UserId = id;

        var (access, exp) = jwt.CreateAccessToken(user);
        var (refresh, refreshExp) = jwt.CreateRefreshToken();

        await repo.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = jwt.HashRefreshToken(refresh),
            ExpiresAt = refreshExp
        }, ct);

        var dto = new UserDto(user.UserId, user.Email, user.FullName, user.Role);
        return (true, new AuthResponse(access, exp, refresh, refreshExp, dto), Array.Empty<string>());
    }

    public async Task<(bool ok, AuthResponse? result, string[] errors)> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await repo.GetByEmailAsync(req.Email.Trim().ToLowerInvariant(), ct);
        if (user is null) return (false, null, new[] { "Invalid credentials." });
        if (!hasher.Verify(req.Password, user.PasswordHash)) return (false, null, new[] { "Invalid credentials." });

        var (access, exp) = jwt.CreateAccessToken(user);
        var (refresh, refreshExp) = jwt.CreateRefreshToken();

        await repo.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = jwt.HashRefreshToken(refresh),
            ExpiresAt = refreshExp
        }, ct);

        var dto = new UserDto(user.UserId, user.Email, user.FullName, user.Role);
        return (true, new AuthResponse(access, exp, refresh, refreshExp, dto), Array.Empty<string>());
    }

    public async Task<(bool ok, AuthResponse? result, string[] errors)> RefreshAsync(Guid userId, string refreshToken, CancellationToken ct = default)
    {
        var user = await repo.GetByIdAsync(userId, ct);
        if (user is null) return (false, null, new[] { "User not found." });

        var tokenHash = jwt.HashRefreshToken(refreshToken);
        var stored = await repo.GetRefreshTokenAsync(userId, tokenHash, ct);
        if (stored is null || !stored.IsActive) return (false, null, new[] { "Invalid refresh token." });

        // revoke old
        stored.RevokedAt = DateTime.UtcNow;
        await repo.RevokeRefreshTokenAsync(stored, ct);

        var (access, exp) = jwt.CreateAccessToken(user);
        var (refresh, refreshExp) = jwt.CreateRefreshToken();

        await repo.AddRefreshTokenAsync(new RefreshToken
        {
            UserId = user.UserId,
            TokenHash = jwt.HashRefreshToken(refresh),
            ExpiresAt = refreshExp
        }, ct);

        var dto = new UserDto(user.UserId, user.Email, user.FullName, user.Role);
        return (true, new AuthResponse(access, exp, refresh, refreshExp, dto), Array.Empty<string>());
    }

    public async Task<UserDto?> GetMeAsync(Guid userId, CancellationToken ct = default)
        => (await repo.GetByIdAsync(userId, ct)) is { } u ? new UserDto(u.UserId, u.Email, u.FullName, u.Role) : null;

    private static List<string> ValidateRegister(RegisterRequest r)
    {
        var errs = new List<string>();
        if (string.IsNullOrWhiteSpace(r.Email)) errs.Add("Email is required.");
        if (string.IsNullOrWhiteSpace(r.Password) || r.Password.Length < 6) errs.Add("Password must be at least 6 characters.");
        if (string.IsNullOrWhiteSpace(r.FullName)) errs.Add("Full name is required.");
        return errs;
    }
}


