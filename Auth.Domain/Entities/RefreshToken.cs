namespace Auth.Domain.Entities;

public class RefreshToken
{
    public long RefreshTokenId { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}
