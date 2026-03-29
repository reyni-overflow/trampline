using FluentAssertions;
using Trampline.Core.Models;

namespace Trampline.Tests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ReturnsTrue()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "hash",
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            CreatedAt = DateTime.UtcNow,
            Revoked = null,
        };

        token.IsActive.Should().BeTrue();
        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenRevoked_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(10),
            Revoked = DateTime.UtcNow,
        };

        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenPastExpiryDate_ReturnsTrue()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            Revoked = null,
        };

        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenBothRevokedAndExpired_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            Revoked = DateTime.UtcNow,
        };

        token.IsActive.Should().BeFalse();
    }
}
