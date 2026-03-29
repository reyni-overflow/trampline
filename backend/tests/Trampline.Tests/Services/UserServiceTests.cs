using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Trampline.Application.Services;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Repositories;

namespace Trampline.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<ILogger<UserService>> _loggerMock = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUser()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateSeed(userId, "test@test.com", "Test", "hash:val", Role.Worker);

        _repoMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(userId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _repoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsUser()
    {
        var user = User.CreateSeed(Guid.NewGuid(), "find@test.com", "Find", "hash:val", Role.Worker);

        _repoMock.Setup(x => x.GetByEmailAsync("find@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetByEmailAsync("find@test.com", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be("find@test.com");
    }

    [Fact]
    public async Task GetByPhoneAsync_ReturnsUser()
    {
        var user = User.CreateSeed(Guid.NewGuid(), "phone@test.com", "Phone", "hash:val", Role.Worker);

        _repoMock.Setup(x => x.GetByPhoneAsync("+79001234567", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetByPhoneAsync("+79001234567", CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailExists_ReturnsFailure()
    {
        var existing = User.CreateSeed(Guid.NewGuid(), "exists@test.com", "Existing", "hash:val", Role.Worker);
        _repoMock.Setup(x => x.GetByEmailAsync("exists@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var request = new RegisterRequest { Email = "exists@test.com", Name = "NewUser", Password = "Password123!" };

        var result = await _sut.CreateUserAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Field == "email");
    }

    [Fact]
    public async Task CreateUserAsync_WhenEmailNew_ReturnsSuccess()
    {
        _repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var request = new RegisterRequest { Email = "new@test.com", Name = "NewUser", Password = "Password123!" };

        var result = await _sut.CreateUserAsync(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("new@test.com");
        _repoMock.Verify(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithInvalidUserData_ReturnsFailure()
    {
        _repoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var request = new RegisterRequest { Email = "invalid", Name = "A", Password = "Password123!" };

        var result = await _sut.CreateUserAsync(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        var id = Guid.NewGuid();

        await _sut.DeleteAsync(id, CancellationToken.None);

        _repoMock.Verify(x => x.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_CallsRepository()
    {
        var user = User.CreateSeed(Guid.NewGuid(), "test@test.com", "Test", "hash:val", Role.Worker);

        await _sut.UpdateAsync(user, CancellationToken.None);

        _repoMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        var users = new List<User>
        {
            User.CreateSeed(Guid.NewGuid(), "a@test.com", "A", "h:v", Role.Worker),
            User.CreateSeed(Guid.NewGuid(), "b@test.com", "B", "h:v", Role.Employee),
        };

        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPaginatedAsync_ReturnsPaginatedResult()
    {
        var users = new List<User> { User.CreateSeed(Guid.NewGuid(), "a@test.com", "A", "h:v", Role.Worker) };
        _repoMock.Setup(x => x.GetPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((users.AsEnumerable(), 1));

        var (items, total) = await _sut.GetPaginatedAsync(1, 10, CancellationToken.None);

        items.Should().HaveCount(1);
        total.Should().Be(1);
    }

    [Fact]
    public async Task GetByRoleAsync_ReturnsFilteredUsers()
    {
        var admins = new List<User> { User.CreateSeed(Guid.NewGuid(), "admin@test.com", "Admin", "h:v", Role.Admin) };
        _repoMock.Setup(x => x.GetByRoleAsync(Role.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admins);

        var result = await _sut.GetByRoleAsync(Role.Admin, CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Role.Should().Be(Role.Admin);
    }

    [Fact]
    public async Task GetByIdsAsync_ReturnsDictionary()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var dict = new Dictionary<Guid, User>
        {
            { id1, User.CreateSeed(id1, "a@test.com", "A", "h:v", Role.Worker) },
            { id2, User.CreateSeed(id2, "b@test.com", "B", "h:v", Role.Worker) },
        };

        _repoMock.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dict);

        var result = await _sut.GetByIdsAsync(new[] { id1, id2 }, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().ContainKey(id1);
    }
}
