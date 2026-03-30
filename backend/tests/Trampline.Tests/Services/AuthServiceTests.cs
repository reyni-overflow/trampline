using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Trampline.Application.Services;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;

namespace Trampline.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ILogger<AuthService>> _loggerMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly Mock<IHostEnvironment> _envMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<Trampline.Core.Storage.IStorageService> _storageMock = new();
    private readonly Mock<IJobRepository> _jobRepoMock = new();
    private readonly Mock<IEventRepository> _eventRepoMock = new();
    private readonly Mock<IMentorshipRepository> _mentorshipRepoMock = new();
    private readonly AuthService _sut;
    private readonly UserAgent _defaultAgent = new("127.0.0.1", "TestAgent");

    public AuthServiceTests()
    {
        _sut = new AuthService(_userServiceMock.Object, _loggerMock.Object,
            _tokenServiceMock.Object, _cacheMock.Object, _envMock.Object, _emailServiceMock.Object, _storageMock.Object,
            _jobRepoMock.Object, _eventRepoMock.Object, _mentorshipRepoMock.Object);
    }

    private User CreateTestUser(Role role = Role.Worker, bool isBlocked = false)
    {
        var user = User.Create("test@example.com", "TestUser", "Password123!", role).Value!;
        if (isBlocked) user.Block();
        return user;
    }

    #region RegisterAsync

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccessWithTokens()
    {
        var request = new RegisterRequest { Email = "new@example.com", Name = "NewUser", Password = "Password123!", Role = Role.Worker };
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(user)).Returns("jwt-token");
        _tokenServiceMock.Setup(x => x.GenerateToken(user, _defaultAgent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "refresh-token" });

        var result = await _sut.RegisterAsync(request, _defaultAgent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("jwt-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        result.Value.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task RegisterAsync_WithAdminRole_ForcesWorkerRole()
    {
        var request = new RegisterRequest { Email = "admin@example.com", Name = "Admin", Password = "Password123!", Role = Role.Admin };

        _userServiceMock.Setup(x => x.CreateUserAsync(It.Is<RegisterRequest>(r => r.Role == Role.Worker), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(CreateTestUser()));
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(It.IsAny<User>())).Returns("token");
        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>(), It.IsAny<UserAgent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "rt" });

        await _sut.RegisterAsync(request, _defaultAgent, CancellationToken.None);

        request.Role.Should().Be(Role.Worker);
    }

    [Fact]
    public async Task RegisterAsync_WhenUserCreationFails_ReturnsFailure()
    {
        var request = new RegisterRequest { Email = "existing@example.com", Name = "User", Password = "Password123!" };

        _userServiceMock.Setup(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Failure(new ErrorDetail("email", "Email уже занят")));

        var result = await _sut.RegisterAsync(request, _defaultAgent, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "email");
    }

    #endregion

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_WithValidEmail_ReturnsSuccess()
    {
        var request = new LoginRequest { Contact = "test@example.com", Password = "Password123!" };
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(user)).Returns("jwt");
        _tokenServiceMock.Setup(x => x.GenerateToken(user, _defaultAgent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "rt" });

        var result = await _sut.LoginAsync(request, _defaultAgent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LoginAsync_WithPhone_UsesPhoneLookup()
    {
        var request = new LoginRequest { Contact = "+79001234567", Password = "Password123!" };
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.GetByPhoneAsync("+79001234567", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(user)).Returns("jwt");
        _tokenServiceMock.Setup(x => x.GenerateToken(user, _defaultAgent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "rt" });

        var result = await _sut.LoginAsync(request, _defaultAgent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userServiceMock.Verify(x => x.GetByPhoneAsync("+79001234567", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithDigitOnlyContact_UsesPhoneLookup()
    {
        var request = new LoginRequest { Contact = "9001234567", Password = "Password123!" };

        _userServiceMock.Setup(x => x.GetByPhoneAsync("9001234567", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(request, _defaultAgent, CancellationToken.None);

        _userServiceMock.Verify(x => x.GetByPhoneAsync("9001234567", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFailure401()
    {
        var request = new LoginRequest { Contact = "nobody@example.com", Password = "Password123!" };

        _userServiceMock.Setup(x => x.GetByEmailAsync("nobody@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(request, _defaultAgent, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 401);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure401()
    {
        var request = new LoginRequest { Contact = "test@example.com", Password = "WrongPassword" };
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync(request, _defaultAgent, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 401);
    }

    [Fact]
    public async Task LoginAsync_WithBlockedUser_ReturnsFailure403()
    {
        var request = new LoginRequest { Contact = "blocked@example.com", Password = "Password123!" };
        var user = CreateTestUser(isBlocked: true);

        _userServiceMock.Setup(x => x.GetByEmailAsync("blocked@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.LoginAsync(request, _defaultAgent, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    [Fact]
    public async Task LoginAsync_TrimsContact()
    {
        var request = new LoginRequest { Contact = "  test@example.com  ", Password = "Password123!" };

        _userServiceMock.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await _sut.LoginAsync(request, _defaultAgent, CancellationToken.None);

        _userServiceMock.Verify(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region RefreshAsync

    [Fact]
    public async Task RefreshAsync_WithValidToken_ReturnsNewTokens()
    {
        var user = CreateTestUser();
        var existingToken = new RefreshToken { Token = "hashed", UserId = user.Id };

        _tokenServiceMock.Setup(x => x.ValidateToken("old-refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(existingToken));
        _tokenServiceMock.Setup(x => x.GetUserFromToken("hashed", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));
        _tokenServiceMock.Setup(x => x.GenerateJwtToken(user)).Returns("new-jwt");
        _tokenServiceMock.Setup(x => x.GenerateToken(user, _defaultAgent, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshToken { Token = "new-refresh" });

        var result = await _sut.RefreshAsync("old-refresh", _defaultAgent, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("new-jwt");
        result.Value.RefreshToken.Should().Be("new-refresh");
    }

    [Fact]
    public async Task RefreshAsync_WithInvalidToken_ReturnsFailure401()
    {
        _tokenServiceMock.Setup(x => x.ValidateToken("invalid", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Failure(new ErrorDetail("token", "invalid")));

        var result = await _sut.RefreshAsync("invalid", _defaultAgent, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 401);
    }

    [Fact]
    public async Task RefreshAsync_UserNotFound_ReturnsFailure401()
    {
        var token = new RefreshToken { Token = "hashed", UserId = Guid.NewGuid() };

        _tokenServiceMock.Setup(x => x.ValidateToken("token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(token));
        _tokenServiceMock.Setup(x => x.GetUserFromToken("hashed", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Failure(new ErrorDetail("user", "not found")));

        var result = await _sut.RefreshAsync("token", _defaultAgent, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshAsync_BlockedUser_ReturnsFailure403()
    {
        var user = CreateTestUser(isBlocked: true);
        var token = new RefreshToken { Token = "hashed", UserId = user.Id };

        _tokenServiceMock.Setup(x => x.ValidateToken("token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RefreshToken>.Success(token));
        _tokenServiceMock.Setup(x => x.GetUserFromToken("hashed", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<User>.Success(user));

        var result = await _sut.RefreshAsync("token", _defaultAgent, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    #endregion

    #region ChangePasswordAsync

    [Fact]
    public async Task ChangePasswordAsync_WithCorrectCurrentPassword_ReturnsSuccess()
    {
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ChangePasswordAsync(user.Id, "Password123!", "NewPassword123!", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userServiceMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ReturnsFailure404()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.ChangePasswordAsync(Guid.NewGuid(), "old", "new12345", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ReturnsFailure()
    {
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ChangePasswordAsync(user.Id, "WrongPassword!", "NewPassword123!", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "currentPassword");
    }

    [Fact]
    public async Task ChangePasswordAsync_ShortNewPassword_ReturnsFailure()
    {
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ChangePasswordAsync(user.Id, "Password123!", "short", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "newPassword");
    }

    [Fact]
    public async Task ChangePasswordAsync_RevokesAllSessions()
    {
        var user = CreateTestUser();
        var session = UserSession.Create(user.Id, "hash", new UserAgent("ip", "agent"));
        user.AddSession(session);

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.ChangePasswordAsync(user.Id, "Password123!", "NewPassword123!", CancellationToken.None);

        user.Sessions.Should().AllSatisfy(s => s.IsRevoked.Should().BeTrue());
    }

    #endregion

    #region DeleteAccountAsync

    [Fact]
    public async Task DeleteAccountAsync_WithCorrectPassword_DeletesUser()
    {
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.DeleteAccountAsync(user.Id, "Password123!", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userServiceMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        user.DeletedAt.Should().NotBeNull();
        user.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAccountAsync_UserNotFound_ReturnsFailure404()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.DeleteAccountAsync(Guid.NewGuid(), "pass", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task DeleteAccountAsync_WrongPassword_ReturnsFailure()
    {
        var user = CreateTestUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.DeleteAccountAsync(user.Id, "WrongPassword", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "password");
    }

    #endregion

    #region ForgotPasswordAsync

    [Fact]
    public async Task ForgotPasswordAsync_WithExistingUser_StoresCodeInCache()
    {
        var user = CreateTestUser();
        _userServiceMock.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _envMock.Setup(x => x.EnvironmentName).Returns("Development");

        var result = await _sut.ForgotPasswordAsync("test@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _cacheMock.Verify(x => x.SetAsync(
            "password_reset:test@example.com",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_InDevelopment_ReturnsCode()
    {
        _userServiceMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestUser());
        _envMock.Setup(x => x.EnvironmentName).Returns("Development");

        var result = await _sut.ForgotPasswordAsync("test@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().MatchRegex(@"^\d{6}$");
    }

    [Fact]
    public async Task ForgotPasswordAsync_InProduction_ReturnsGenericMessage()
    {
        _userServiceMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestUser());
        _envMock.Setup(x => x.EnvironmentName).Returns("Production");
        _emailServiceMock.Setup(x => x.IsConfigured).Returns(true);

        var result = await _sut.ForgotPasswordAsync("test@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Код отправлен");
    }

    [Fact]
    public async Task ForgotPasswordAsync_NonExistentEmail_StillReturnsSuccess()
    {
        _userServiceMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _envMock.Setup(x => x.EnvironmentName).Returns("Production");

        var result = await _sut.ForgotPasswordAsync("nobody@example.com", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region ResetPasswordAsync

    [Fact]
    public async Task ResetPasswordAsync_WithValidCode_ResetsPassword()
    {
        var user = CreateTestUser();
        _cacheMock.Setup(x => x.GetAsync("password_reset_attempts:test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        _cacheMock.Setup(x => x.GetAsync("password_reset:test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("123456"));
        _userServiceMock.Setup(x => x.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.ResetPasswordAsync("test@example.com", "123456", "NewPassword123!", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userServiceMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_TooManyAttempts_ReturnsFailure429()
    {
        _cacheMock.Setup(x => x.GetAsync("password_reset_attempts:test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("5"));

        var result = await _sut.ResetPasswordAsync("test@example.com", "123456", "NewPassword123!", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 429);
    }

    [Fact]
    public async Task ResetPasswordAsync_InvalidCode_IncrementsAttempts()
    {
        _cacheMock.Setup(x => x.GetAsync("password_reset_attempts:test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        _cacheMock.Setup(x => x.GetAsync("password_reset:test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("654321"));

        var result = await _sut.ResetPasswordAsync("test@example.com", "123456", "NewPassword123!", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        _cacheMock.Verify(x => x.SetAsync(
            "password_reset_attempts:test@example.com",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_ShortNewPassword_ReturnsFailure()
    {
        _cacheMock.Setup(x => x.GetAsync("password_reset_attempts:test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);
        _cacheMock.Setup(x => x.GetAsync("password_reset:test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes("123456"));

        var result = await _sut.ResetPasswordAsync("test@example.com", "123456", "short", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "newPassword");
    }

    #endregion
}
