using System.ComponentModel.DataAnnotations;

namespace Trampline.Core.Models;

public class UserSession
{
    [Key]
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    [MaxLength(255)]
    public string? DeviceName { get; private set; }

    public UserAgent UserAgent { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    public DateTime? LastUsedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    [MaxLength(100)]
    public string? RevocationReason { get; private set; }

    public bool IsRevoked => RevokedAt != null;

    public bool IsExpired => ExpiresAt < DateTime.UtcNow;

    public bool IsActive => !IsRevoked && !IsExpired;

    public User User { get; private set; } = null!;

    private UserSession() { }

    public static UserSession Create(Guid userId, string tokenHash, UserAgent userAgent)
    {
        return new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(10)
        };
    }

    public void MarkAsUsed()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    public void Revoke(string reason)
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
        RevocationReason = reason;
    }
}