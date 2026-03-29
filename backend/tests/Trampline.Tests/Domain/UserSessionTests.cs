using FluentAssertions;
using Trampline.Core.Models;

namespace Trampline.Tests.Domain;

public class UserSessionTests
{
    [Fact]
    public void Create_SetsCorrectProperties()
    {
        var userId = Guid.NewGuid();
        var tokenHash = "somehash";
        var agent = new UserAgent("192.168.1.1", "Mozilla/5.0");

        var session = UserSession.Create(userId, tokenHash, agent);

        session.Id.Should().NotBeEmpty();
        session.UserId.Should().Be(userId);
        session.TokenHash.Should().Be(tokenHash);
        session.UserAgent.Should().Be(agent);
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(10), TimeSpan.FromSeconds(5));
        session.RevokedAt.Should().BeNull();
        session.RevocationReason.Should().BeNull();
        session.LastUsedAt.Should().BeNull();
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ReturnsTrue()
    {
        var session = UserSession.Create(Guid.NewGuid(), "hash", new UserAgent("127.0.0.1", "Agent"));

        session.IsActive.Should().BeTrue();
        session.IsRevoked.Should().BeFalse();
        session.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Revoke_SetsRevokedAtAndReason()
    {
        var session = UserSession.Create(Guid.NewGuid(), "hash", new UserAgent("127.0.0.1", "Agent"));

        session.Revoke("PasswordChanged");

        session.IsRevoked.Should().BeTrue();
        session.IsActive.Should().BeFalse();
        session.RevokedAt.Should().NotBeNull();
        session.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        session.RevocationReason.Should().Be("PasswordChanged");
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_DoesNotUpdate()
    {
        var session = UserSession.Create(Guid.NewGuid(), "hash", new UserAgent("127.0.0.1", "Agent"));
        session.Revoke("FirstRevoke");
        var firstRevokedAt = session.RevokedAt;

        session.Revoke("SecondRevoke");

        session.RevokedAt.Should().Be(firstRevokedAt);
        session.RevocationReason.Should().Be("FirstRevoke");
    }

    [Fact]
    public void MarkAsUsed_UpdatesLastUsedAt()
    {
        var session = UserSession.Create(Guid.NewGuid(), "hash", new UserAgent("127.0.0.1", "Agent"));

        session.MarkAsUsed();

        session.LastUsedAt.Should().NotBeNull();
        session.LastUsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
