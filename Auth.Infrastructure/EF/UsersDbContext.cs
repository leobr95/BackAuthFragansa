using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.EF;

public class UsersDbContext(DbContextOptions<UsersDbContext> opts) : DbContext(opts)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Opcional: usar schema "auth" (similar a Search Path=auth en Postgres)
        b.HasDefaultSchema("auth");

        b.Entity<User>(eb =>
        {
            eb.ToTable("users");
            eb.HasKey(x => x.UserId);

            eb.Property(x => x.UserId)
              .HasColumnName("user_id")
              .HasColumnType("uniqueidentifier");

            eb.Property(x => x.Email)
              .HasColumnName("email")
              .HasMaxLength(150)
              .IsRequired();

            eb.HasIndex(x => x.Email).IsUnique();

            eb.Property(x => x.PasswordHash)
              .HasColumnName("password_hash")
              .IsRequired();

            eb.Property(x => x.FullName)
              .HasColumnName("full_name")
              .HasMaxLength(150)
              .IsRequired();

            eb.Property(x => x.Role)
              .HasColumnName("role")
              .HasMaxLength(50)
              .IsRequired();

            eb.Property(x => x.CreatedAt)
              .HasColumnName("created_at")
              .HasColumnType("datetime2")
              .HasDefaultValueSql("SYSUTCDATETIME()");

            eb.Property(x => x.UpdatedAt)
              .HasColumnName("updated_at")
              .HasColumnType("datetime2")
              .HasDefaultValueSql("SYSUTCDATETIME()");
        });

        b.Entity<RefreshToken>(eb =>
        {
            eb.ToTable("refresh_tokens");
            eb.HasKey(x => x.RefreshTokenId);

            eb.Property(x => x.RefreshTokenId)
              .HasColumnName("refresh_token_id")
              .ValueGeneratedOnAdd(); // SQL Server IDENTITY

            eb.Property(x => x.UserId)
              .HasColumnName("user_id")
              .HasColumnType("uniqueidentifier")
              .IsRequired();

            // SHA256 base64 suele quedar ~44 chars
            eb.Property(x => x.TokenHash)
              .HasColumnName("token_hash")
              .HasMaxLength(88)
              .IsRequired();

            eb.Property(x => x.ExpiresAt)
              .HasColumnName("expires_at")
              .HasColumnType("datetime2")
              .IsRequired();

            eb.Property(x => x.CreatedAt)
              .HasColumnName("created_at")
              .HasColumnType("datetime2")
              .HasDefaultValueSql("SYSUTCDATETIME()");

            eb.Property(x => x.RevokedAt)
              .HasColumnName("revoked_at")
              .HasColumnType("datetime2");

            eb.HasIndex(x => new { x.UserId, x.TokenHash }).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var e in ChangeTracker.Entries<User>())
        {
            if (e.State == EntityState.Modified) e.Entity.Touch();
        }

        return base.SaveChangesAsync(ct);
    }
}
