using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Trampline.Application.Services;
using Trampline.Application.Services.IO;
using Trampline.Core.Models;
using Trampline.Core.Options;
using Trampline.Core.Repositories;
using Trampline.Shared.Services;

namespace Trampline.Tests.Services;

public class SecurityTests
{
    private readonly Mock<ILogger<MediaService>> _mediaLoggerMock = new();
    private readonly Mock<Trampline.Core.Storage.IStorageService> _storageMock = new();
    private readonly MediaService _mediaSut;

    private readonly Mock<ILogger<TokenService>> _tokenLoggerMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IInfoService> _infoServiceMock = new();
    private readonly Mock<IUserSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly TokenService _tokenSut;

    public SecurityTests()
    {
        _storageMock.Setup(s => s.UploadAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stream _, string path, string _, CancellationToken _) => path);
        _mediaSut = new MediaService(_mediaLoggerMock.Object, _storageMock.Object);

        var jwtOption = new JwtOption
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "this-is-a-very-long-secret-key-for-testing-purposes-minimum-32-chars"
        };

        _tokenSut = new TokenService(_tokenLoggerMock.Object, _refreshRepoMock.Object,
            _userServiceMock.Object, Options.Create(jwtOption), _infoServiceMock.Object,
            _sessionRepoMock.Object, _cacheMock.Object);
    }

    private static User CreateTestUser(Role role = Role.Worker)
    {
        return User.Create("test@example.com", "TestUser", "Password123!", role).Value!;
    }

    #region Path Traversal Protection

    [Theory]
    [InlineData("../../etc/passwd")]
    [InlineData("../file.txt")]
    [InlineData("..\\..\\windows\\system32\\config")]
    [InlineData("/files/../../../etc/shadow")]
    public async Task DeleteFile_PathTraversalAttempt_ReturnsFailure(string maliciousPath)
    {
        var result = await _mediaSut.DeleteFile(maliciousPath, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteFile_EmptyPath_ReturnsFailure()
    {
        var result = await _mediaSut.DeleteFile("", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("empty"));
    }

    [Fact]
    public async Task DeleteFile_NullPath_ReturnsFailure()
    {
        var result = await _mediaSut.DeleteFile(null!, TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Malicious Content Scanner

    [Fact]
    public async Task UploadFile_WithScriptTag_RejectsFile()
    {
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x00, 0x00 };
        var scriptContent = Encoding.UTF8.GetBytes("<script>alert('xss')</script>");
        var fullContent = pngHeader.Concat(scriptContent).ToArray();

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.Length).Returns(fullContent.Length);
        fileMock.Setup(f => f.FileName).Returns("malicious.png");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(fullContent));

        var result = await _mediaSut.UploadFile(fileMock.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("suspicious"));
    }

    [Fact]
    public async Task UploadFile_WithPhpTag_RejectsFile()
    {
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x00, 0x00 };
        var phpContent = Encoding.UTF8.GetBytes("<?php system('rm -rf /'); ?>");
        var fullContent = pngHeader.Concat(phpContent).ToArray();

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.Length).Returns(fullContent.Length);
        fileMock.Setup(f => f.FileName).Returns("malicious.png");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(fullContent));

        var result = await _mediaSut.UploadFile(fileMock.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("suspicious"));
    }

    [Fact]
    public async Task UploadFile_WithIframeTag_RejectsFile()
    {
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x00, 0x00 };
        var iframeContent = Encoding.UTF8.GetBytes("<iframe src='http://evil.com'></iframe>");
        var fullContent = pngHeader.Concat(iframeContent).ToArray();

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.Length).Returns(fullContent.Length);
        fileMock.Setup(f => f.FileName).Returns("malicious.png");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(fullContent));

        var result = await _mediaSut.UploadFile(fileMock.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("suspicious"));
    }

    [Fact]
    public async Task UploadFile_CleanImage_AcceptsFile()
    {
        var pngHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x00, 0x00 };

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.Length).Returns(pngHeader.Length);
        fileMock.Setup(f => f.FileName).Returns("clean.png");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(pngHeader));

        var result = await _mediaSut.UploadFile(fileMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region JWT Blacklist

    [Fact]
    public async Task BlacklistJwtAsync_ValidToken_StoresInCache()
    {
        var user = CreateTestUser();
        var jwt = _tokenSut.GenerateJwtToken(user);

        await _tokenSut.BlacklistJwtAsync(jwt, CancellationToken.None);

        _cacheMock.Verify(x => x.SetAsync(
            It.Is<string>(k => k.StartsWith("jwt_blacklist:")),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsJwtBlacklistedAsync_WhenBlacklisted_ReturnsTrue()
    {
        var jti = Guid.NewGuid().ToString();

        _cacheMock.Setup(x => x.GetAsync($"jwt_blacklist:{jti}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Encoding.UTF8.GetBytes("1"));

        var result = await _tokenSut.IsJwtBlacklistedAsync(jti, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsJwtBlacklistedAsync_WhenNotBlacklisted_ReturnsFalse()
    {
        var jti = Guid.NewGuid().ToString();

        _cacheMock.Setup(x => x.GetAsync($"jwt_blacklist:{jti}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var result = await _tokenSut.IsJwtBlacklistedAsync(jti, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsJwtBlacklistedAsync_EmptyJti_ReturnsFalse()
    {
        var result = await _tokenSut.IsJwtBlacklistedAsync("", CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsJwtBlacklistedAsync_NullJti_ReturnsFalse()
    {
        var result = await _tokenSut.IsJwtBlacklistedAsync(null!, CancellationToken.None);

        result.Should().BeFalse();
    }

    #endregion

    #region JTI Claim in JWT

    [Fact]
    public void GenerateJwtToken_ContainsJtiClaim()
    {
        var user = CreateTestUser();

        var token = _tokenSut.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
        var jti = jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        jti.Should().NotBeNullOrEmpty();
        Guid.TryParse(jti, out _).Should().BeTrue();
    }

    [Fact]
    public void GenerateJwtToken_DifferentCalls_ProduceDifferentJti()
    {
        var user = CreateTestUser();
        var handler = new JwtSecurityTokenHandler();

        var token1 = _tokenSut.GenerateJwtToken(user);
        var token2 = _tokenSut.GenerateJwtToken(user);

        var jwt1 = handler.ReadJwtToken(token1);
        var jwt2 = handler.ReadJwtToken(token2);

        var jti1 = jwt1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwt2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jti1.Should().NotBe(jti2);
    }

    #endregion
}
