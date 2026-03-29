using FluentAssertions;
using Trampline.Core.Models;

namespace Trampline.Tests.Domain;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = PasswordHasher.Hash("Password123");

        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_ContainsSaltSeparator()
    {
        var hash = PasswordHasher.Hash("Password123");

        hash.Should().Contain(":");
        hash.Split(':').Should().HaveCount(2);
    }

    [Fact]
    public void Hash_ProducesDifferentHashesForSamePassword()
    {
        var hash1 = PasswordHasher.Hash("Password123");
        var hash2 = PasswordHasher.Hash("Password123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var password = "SecurePassword123!";
        var hash = PasswordHasher.Hash(password);

        var result = PasswordHasher.Verify(password, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("CorrectPassword");

        var result = PasswordHasher.Verify("WrongPassword", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithInvalidHashFormat_ReturnsFalse()
    {
        var result = PasswordHasher.Verify("Password123", "invalidhash");

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("Password123");

        var result = PasswordHasher.Verify("", hash);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("VeryLongPasswordThatShouldAlsoWork12345!@#$%")]
    [InlineData("Пароль_с_кириллицей_123")]
    [InlineData("P@$$w0rd!#%")]
    public void Hash_And_Verify_WorksWithVariousPasswords(string password)
    {
        var hash = PasswordHasher.Hash(password);

        PasswordHasher.Verify(password, hash).Should().BeTrue();
    }

    [Fact]
    public void HashToken_ReturnsConsistentHash()
    {
        var token = "sk-test-token-123";

        var hash1 = PasswordHasher.HashToken(token);
        var hash2 = PasswordHasher.HashToken(token);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashToken_DifferentTokens_ProduceDifferentHashes()
    {
        var hash1 = PasswordHasher.HashToken("token1");
        var hash2 = PasswordHasher.HashToken("token2");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyToken_WithCorrectToken_ReturnsTrue()
    {
        var token = "sk-test-token-456";
        var hash = PasswordHasher.HashToken(token);

        var result = PasswordHasher.VerifyToken(token, hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyToken_WithWrongToken_ReturnsFalse()
    {
        var hash = PasswordHasher.HashToken("correct-token");

        var result = PasswordHasher.VerifyToken("wrong-token", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void HashToken_ReturnsBase64String()
    {
        var hash = PasswordHasher.HashToken("test-token");

        var act = () => Convert.FromBase64String(hash);
        act.Should().NotThrow();
    }
}
