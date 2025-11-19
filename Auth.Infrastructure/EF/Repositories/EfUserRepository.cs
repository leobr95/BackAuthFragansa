using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.EF.Repositories;

public class EfUserRepository(UsersDbContext db) : IUserRepository
{
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
        => await db.Users.AsNoTracking().AnyAsync(x => x.Email == email, ct);

    public async Task<Guid> CreateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user.UserId;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == id, ct);

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        return await db.SaveChangesAsync(ct) > 0;
    }

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        db.RefreshTokens.Add(token);
        await db.SaveChangesAsync(ct);
    }

    public Task<RefreshToken?> GetRefreshTokenAsync(Guid userId, string tokenHash, CancellationToken ct = default)
        => db.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.TokenHash == tokenHash, ct);

    public async Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        token.RevokedAt = DateTime.UtcNow;
        db.RefreshTokens.Update(token);
        await db.SaveChangesAsync(ct);
    }
}
