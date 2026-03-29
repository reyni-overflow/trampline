using System.ComponentModel.DataAnnotations;

namespace Trampline.Core.Models;

public class RefreshToken
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? Revoked { get; set; }

    public string Token { get; set; } = string.Empty;

    public bool IsActive => Revoked == null && !IsExpired;

    public bool IsExpired => ExpiresAt < DateTime.UtcNow;

    public string Agent { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;
}