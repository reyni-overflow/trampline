using FluentAssertions;
using Trampline.Core.Exceptions;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Models.Worker;

namespace Trampline.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ReturnsSuccessResult()
    {
        var result = User.Create("test@example.com", "TestUser", "Password123", Role.Worker);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Email.Should().Be("test@example.com");
        result.Value.Nickname.Should().Be("TestUser");
        result.Value.Role.Should().Be(Role.Worker);
        result.Value.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithEmployeeRole_ReturnsSuccessResult()
    {
        var result = User.Create("employer@company.com", "CompanyUser", "Password123", Role.Employee);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be(Role.Employee);
    }

    [Fact]
    public void Create_WithAdminRole_ReturnsSuccessResult()
    {
        var result = User.Create("admin@platform.com", "AdminUser", "Password123", Role.Admin);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Role.Should().Be(Role.Admin);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyEmail_ReturnsFailure(string email)
    {
        var result = User.Create(email, "TestUser", "Password123", Role.Worker);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "email");
    }

    [Fact]
    public void Create_WithInvalidEmailFormat_ReturnsFailure()
    {
        var result = User.Create("notanemail", "TestUser", "Password123", Role.Worker);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "email" && e.Message.Contains("email"));
    }

    [Theory]
    [InlineData("A")]
    [InlineData("")]
    public void Create_WithShortNickname_ReturnsFailure(string name)
    {
        var result = User.Create("test@example.com", name, "Password123", Role.Worker);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "name");
    }

    [Fact]
    public void Create_TrimsEmailAndNickname()
    {
        var result = User.Create("  test@example.com  ", "  TestUser  ", "Password123", Role.Worker);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("test@example.com");
        result.Value.Nickname.Should().Be("TestUser");
    }

    [Fact]
    public void Create_HashesPassword()
    {
        var result = User.Create("test@example.com", "TestUser", "Password123", Role.Worker);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PasswordHash.Should().NotBe("Password123");
        result.Value.PasswordHash.Should().Contain(":");
    }

    [Fact]
    public void Create_SetsDefaultValues()
    {
        var result = User.Create("test@example.com", "TestUser", "Password123", Role.Worker);

        result.Value!.IsPrivate.Should().BeFalse();
        result.Value.Phone.Should().BeNull();
        result.Value.Avatar.Should().BeNull();
        result.Value.WorkerProfile.Should().BeNull();
        result.Value.EmployeeProfile.Should().BeNull();
        result.Value.Sessions.Should().BeEmpty();
    }

    [Fact]
    public void CreateSeed_CreatesUserWithGivenId()
    {
        var id = Guid.NewGuid();
        var user = User.CreateSeed(id, "seed@test.com", "Seed", "hash:value", Role.Admin);

        user.Id.Should().Be(id);
        user.Email.Should().Be("seed@test.com");
        user.Nickname.Should().Be("Seed");
        user.PasswordHash.Should().Be("hash:value");
        user.Role.Should().Be(Role.Admin);
    }

    [Fact]
    public void ChangeRole_UpdatesRole()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;

        user.ChangeRole(Role.Employee);

        user.Role.Should().Be(Role.Employee);
    }

    [Fact]
    public void ChangePrivate_TogglesIsPrivate()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;

        user.IsPrivate.Should().BeFalse();
        user.ChangePrivate();
        user.IsPrivate.Should().BeTrue();
        user.ChangePrivate();
        user.IsPrivate.Should().BeFalse();
    }

    [Fact]
    public void SetPrivate_SetsExactValue()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;

        user.SetPrivate(true);
        user.IsPrivate.Should().BeTrue();

        user.SetPrivate(false);
        user.IsPrivate.Should().BeFalse();
    }

    [Fact]
    public void SetPhone_UpdatesPhone()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;

        user.SetPhone("+79001234567");
        user.Phone.Should().Be("+79001234567");

        user.SetPhone(null);
        user.Phone.Should().BeNull();
    }

    [Fact]
    public void SetAvatar_UpdatesAvatar()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;

        user.SetAvatar("/files/photos/avatar.jpg");
        user.Avatar.Should().Be("/files/photos/avatar.jpg");
    }

    [Fact]
    public void ChangePassword_UpdatesPasswordHash()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;
        var oldHash = user.PasswordHash;

        user.ChangePassword("newhash:value");
        user.PasswordHash.Should().Be("newhash:value");
        user.PasswordHash.Should().NotBe(oldHash);
    }

    [Fact]
    public void AddSession_AddsSessionToCollection()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;
        var session = UserSession.Create(user.Id, "tokenhash", new UserAgent("127.0.0.1", "Test Agent"));

        user.AddSession(session);

        user.Sessions.Should().HaveCount(1);
        user.Sessions.Should().Contain(session);
    }

    [Fact]
    public void AddSession_WithDifferentUserId_ThrowsDomainException()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;
        var session = UserSession.Create(Guid.NewGuid(), "tokenhash", new UserAgent("127.0.0.1", "Test Agent"));

        var act = () => user.AddSession(session);

        act.Should().Throw<DomainException>().WithMessage("*another user*");
    }

    [Fact]
    public void RemoveSession_RemovesFromCollection()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;
        var session = UserSession.Create(user.Id, "tokenhash", new UserAgent("127.0.0.1", "Test Agent"));
        user.AddSession(session);

        user.RemoveSession(session);

        user.Sessions.Should().BeEmpty();
    }

    [Fact]
    public void RevokeAllSessions_RevokesAllSessions()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;
        var session1 = UserSession.Create(user.Id, "hash1", new UserAgent("127.0.0.1", "Agent1"));
        var session2 = UserSession.Create(user.Id, "hash2", new UserAgent("127.0.0.1", "Agent2"));
        user.AddSession(session1);
        user.AddSession(session2);

        user.RevokeAllSessions();

        user.Sessions.Should().AllSatisfy(s => s.IsRevoked.Should().BeTrue());
    }

    [Fact]
    public void GetActiveSessions_ReturnsOnlyActiveSessions()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;
        var activeSession = UserSession.Create(user.Id, "hash1", new UserAgent("127.0.0.1", "Agent1"));
        var revokedSession = UserSession.Create(user.Id, "hash2", new UserAgent("127.0.0.1", "Agent2"));
        user.AddSession(activeSession);
        user.AddSession(revokedSession);
        revokedSession.Revoke("test");

        var activeSessions = user.GetActiveSessions().ToList();

        activeSessions.Should().HaveCount(1);
        activeSessions.Should().Contain(activeSession);
    }

    [Fact]
    public void SetWorkerProfile_AssignsProfile()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Worker).Value!;
        var profile = WorkerProfile.Create(user.Id, "Иван", "Иванов", "Иванович", null, null, null);

        user.SetWorkerProfile(profile);

        user.WorkerProfile.Should().Be(profile);
    }

    [Fact]
    public void SetEmployeeProfile_AssignsProfile()
    {
        var user = User.Create("test@example.com", "TestUser", "Password123", Role.Employee).Value!;
        var info = new EmployeeInfo { INN = "1234567890", Email = "company@test.com" };
        var profile = EmployeeProfile.Create(user.Id, "Company", "Desc", "IT", info, "https://site.com");

        user.SetEmployeeProfile(profile);

        user.EmployeeProfile.Should().Be(profile);
    }

    [Fact]
    public void Create_WithMultipleValidationErrors_ReturnsAllErrors()
    {
        var result = User.Create("", "A", "Password123", Role.Worker);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
