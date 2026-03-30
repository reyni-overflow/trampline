using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Trampline.Application.Services;
using Trampline.Core.Models;
using Trampline.Core.Options;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;
using Trampline.Shared.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Trampline.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<ILogger<TokenService>> _loggerMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IInfoService> _infoServiceMock = new();
    private readonly Mock<IUserSessionRepository> _sessionRepoMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly JwtOption _jwtOption;
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        _jwtOption = new JwtOption
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            Key = "this-is-a-very-long-secret-key-for-testing-purposes-minimum-32-chars"
        };
        var optionsMock = Options.Create(_jwtOption);

        _sut = new TokenService(_loggerMock.Object, _refreshRepoMock.Object,
            _userServiceMock.Object, optionsMock, _infoServiceMock.Object, _sessionRepoMock.Object, _cacheMock.Object);
    }

    private static User CreateTestUser(Role role = Role.Worker)
    {
        return User.Create("test@example.com", "TestUser", "Password123!", role).Value!;
    }

    #region GenerateJwtToken

    [Fact]
    public void GenerateJwtToken_ReturnsValidJwtString()
    {
        var user = CreateTestUser();

        var token = _sut.GenerateJwtToken(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateJwtToken_ContainsCorrectClaims()
    {
        var user = CreateTestUser();

        var token = _sut.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == user.Role.ToString());
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateJwtToken_SetsCorrectIssuerAndAudience()
    {
        var user = CreateTestUser();

        var token = _sut.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().Contain("test-audience");
    }

    [Fact]
    public void GenerateJwtToken_ExpiresIn30Minutes()
    {
        var user = CreateTestUser();

        var token = _sut.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void GenerateJwtToken_IncludesRoleForEmployee()
    {
        var user = CreateTestUser(Role.Employee);

        var token = _sut.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Employee");
    }

    [Fact]
    public void GenerateJwtToken_IncludesRoleForAdmin()
    {
        var user = CreateTestUser(Role.Admin);

        var token = _sut.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateJwtToken_WithAvatar_IncludesAvatarClaim()
    {
        var user = CreateTestUser();
        user.SetAvatar("/files/photos/avatar.jpg");

        var token = _sut.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "avatar" && c.Value == "/files/photos/avatar.jpg");
    }

    [Fact]
    public void GenerateJwtToken_WithoutAvatar_AvatarClaimIsEmpty()
    {
        var user = CreateTestUser();

        var token = _sut.GenerateJwtToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "avatar" && c.Value == "");
    }

    [Fact]
    public void GenerateJwtToken_DifferentUsers_ProduceDifferentTokens()
    {
        var user1 = CreateTestUser();
        var user2 = User.Create("other@example.com", "OtherUser", "Password123!", Role.Employee).Value!;

        var token1 = _sut.GenerateJwtToken(user1);
        var token2 = _sut.GenerateJwtToken(user2);

        token1.Should().NotBe(token2);
    }

    #endregion

    #region ValidateToken

    [Fact]
    public async Task ValidateToken_WithNullToken_ReturnsFailure()
    {
        var result = await _sut.ValidateToken(null, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "token");
    }

    [Fact]
    public async Task ValidateToken_WithEmptyToken_ReturnsFailure()
    {
        var result = await _sut.ValidateToken("", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateToken_TokenNotFound_ReturnsFailure()
    {
        _refreshRepoMock.Setup(x => x.GetByTokenAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Failure(new ErrorDetail("token", "not found")));

        var result = await _sut.ValidateToken("unknown", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateToken_ExpiredToken_RevokesAndReturnsFailure()
    {
        var expiredToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "expired-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-11),
            UserId = Guid.NewGuid(),
        };

        _refreshRepoMock.Setup(x => x.GetByTokenAsync("expired", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(expiredToken));

        var result = await _sut.ValidateToken("expired", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("no longer active"));
        _refreshRepoMock.Verify(x => x.UpdateAsync(expiredToken, It.IsAny<CancellationToken>()), Times.Once);
        expiredToken.Revoked.Should().NotBeNull();
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsSuccess()
    {
        var validToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "valid-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow,
            UserId = Guid.NewGuid(),
        };

        _refreshRepoMock.Setup(x => x.GetByTokenAsync("valid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(validToken));

        var result = await _sut.ValidateToken("valid", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(validToken);
    }

    #endregion

    #region GenerateToken

    [Fact]
    public async Task GenerateToken_CreatesRefreshTokenAndSession()
    {
        var user = CreateTestUser();
        var agent = new UserAgent("192.168.1.1", "Mozilla/5.0");

        _infoServiceMock.Setup(x => x.GetLocation("192.168.1.1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Moscow, Russia");

        var result = await _sut.GenerateToken(user, agent, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Agent.Should().Be("Mozilla/5.0");
        result.Location.Should().Be("Moscow, Russia");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(10));

        _refreshRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _sessionRepoMock.Verify(x => x.AddAsync(It.IsAny<UserSession>(), It.IsAny<CancellationToken>()), Times.Once);
        _userServiceMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateToken_WithUnknownDevice_SkipsGeoLookup()
    {
        var user = CreateTestUser();
        var agent = new UserAgent("Unknown device", "TestAgent");

        var result = await _sut.GenerateToken(user, agent, CancellationToken.None);

        result.Location.Should().Be("Unknown device");
        _infoServiceMock.Verify(x => x.GetLocation(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GenerateToken_TokenFormatStartsWithSk()
    {
        var user = CreateTestUser();
        var agent = new UserAgent("Unknown device", "Agent");

        RefreshToken? capturedToken = null;
        _refreshRepoMock.Setup(x => x.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((t, _) => capturedToken = t);

        await _sut.GenerateToken(user, agent, CancellationToken.None);

        capturedToken.Should().NotBeNull();
        capturedToken!.Token.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region GetUserFromToken

    [Fact]
    public async Task GetUserFromToken_TokenFound_ReturnsUser()
    {
        var user = CreateTestUser();
        var token = new RefreshToken { Token = "hash", UserId = user.Id };

        _refreshRepoMock.Setup(x => x.GetByTokenAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(token));
        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetUserFromToken("hash", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetUserFromToken_TokenNotFound_ReturnsFailure()
    {
        _refreshRepoMock.Setup(x => x.GetByTokenAsync("bad", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Failure(new ErrorDetail("token", "not found")));

        var result = await _sut.GetUserFromToken("bad", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserFromToken_UserNotFound_ReturnsFailure404()
    {
        var token = new RefreshToken { Token = "hash", UserId = Guid.NewGuid() };

        _refreshRepoMock.Setup(x => x.GetByTokenAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(token));
        _userServiceMock.Setup(x => x.GetByIdAsync(token.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetUserFromToken("hash", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion

    #region GetUserFromJwtToken

    [Fact]
    public async Task GetUserFromJwtToken_ValidJwt_ReturnsUser()
    {
        var user = CreateTestUser();
        var jwt = _sut.GenerateJwtToken(user);

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetUserFromJwtToken(jwt, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetUserFromJwtToken_UserNotFound_ReturnsFailure()
    {
        var user = CreateTestUser();
        var jwt = _sut.GenerateJwtToken(user);

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetUserFromJwtToken(jwt, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion

    #region GetRoleFromJwtToken

    [Fact]
    public void GetRoleFromJwtToken_ValidJwt_ReturnsRole()
    {
        var user = CreateTestUser(Role.Admin);
        var jwt = _sut.GenerateJwtToken(user);

        var role = _sut.GetRoleFromJwtToken(jwt);

        role.Should().Be("Admin");
    }

    [Fact]
    public void GetRoleFromJwtToken_WorkerRole_ReturnsWorker()
    {
        var user = CreateTestUser(Role.Worker);
        var jwt = _sut.GenerateJwtToken(user);

        var role = _sut.GetRoleFromJwtToken(jwt);

        role.Should().Be("Worker");
    }

    [Fact]
    public void GetRoleFromJwtToken_EmployeeRole_ReturnsEmployee()
    {
        var user = CreateTestUser(Role.Employee);
        var jwt = _sut.GenerateJwtToken(user);

        var role = _sut.GetRoleFromJwtToken(jwt);

        role.Should().Be("Employee");
    }

    #endregion

    #region GetSessions

    [Fact]
    public async Task GetSession_DelegatestoRepository()
    {
        var token = new RefreshToken { Id = Guid.NewGuid(), Token = "hash" };
        _refreshRepoMock.Setup(x => x.GetByTokenAsync("hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(token));

        var result = await _sut.GetSession("hash", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetSessions_ReturnsTokensForUser()
    {
        var userId = Guid.NewGuid();
        var tokens = new List<RefreshToken> { new() { Id = Guid.NewGuid(), UserId = userId } };

        _refreshRepoMock.Setup(x => x.GetTokensAsync(userId.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<RefreshToken>>.Success(tokens));

        var result = await _sut.GetSessions(userId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    #endregion

    #region GetTokensByUserIdAsync

    [Fact]
    public async Task GetTokensByUserIdAsync_UserExists_ReturnsActiveSessions()
    {
        var user = CreateTestUser();
        var session = UserSession.Create(user.Id, "hash", new UserAgent("127.0.0.1", "Agent"));
        user.AddSession(session);

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetTokensByUserIdAsync(user.Id, CancellationToken.None);

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTokensByUserIdAsync_UserNotFound_ReturnsEmpty()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetTokensByUserIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTokensByUserIdAsync_FiltersRevokedSessions()
    {
        var user = CreateTestUser();
        var activeSession = UserSession.Create(user.Id, "hash1", new UserAgent("ip", "agent"));
        var revokedSession = UserSession.Create(user.Id, "hash2", new UserAgent("ip", "agent"));
        user.AddSession(activeSession);
        user.AddSession(revokedSession);
        revokedSession.Revoke("test");

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = (await _sut.GetTokensByUserIdAsync(user.Id, CancellationToken.None)).ToList();

        result.Should().HaveCount(1);
        result.Should().Contain(activeSession);
    }

    #endregion

    #region DeleteTokenAsync

    [Fact]
    public async Task DeleteTokenAsync_TokenExists_DeletesIt()
    {
        var token = new RefreshToken { Token = "hash" };
        _refreshRepoMock.Setup(x => x.GetByTokenAsync("to-delete", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(token));

        await _sut.DeleteTokenAsync("to-delete", CancellationToken.None);

        _refreshRepoMock.Verify(x => x.DeleteAsync("hash", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTokenAsync_TokenNotFound_DoesNothing()
    {
        _refreshRepoMock.Setup(x => x.GetByTokenAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Failure(new ErrorDetail("token", "not found")));

        await _sut.DeleteTokenAsync("missing", CancellationToken.None);

        _refreshRepoMock.Verify(x => x.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region DisableTokenAsync

    [Fact]
    public async Task DisableTokenAsync_DelegatesToRepository()
    {
        await _sut.DisableTokenAsync("token-to-disable", CancellationToken.None);

        _refreshRepoMock.Verify(x => x.DisableTokenAsync("token-to-disable", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
