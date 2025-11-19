using Auth.Domain.Entities;

namespace Auth.Domain.Interfaces;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task<Guid> CreateAsync(User user, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> UpdateAsync(User user, CancellationToken ct = default);

    // Refresh tokens
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    Task<RefreshToken?> GetRefreshTokenAsync(Guid userId, string tokenHash, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
}
