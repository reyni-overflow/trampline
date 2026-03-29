using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Trampline.Application.Services;
using Trampline.Application.Services.Employees;
using Trampline.Application.Services.IO;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;

namespace Trampline.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<ILogger<EmployeeService>> _loggerMock = new();
    private readonly Mock<IMediaService> _mediaServiceMock = new();
    private readonly Mock<IDaDataService> _daDataMock = new();
    private readonly EmployeeService _sut;

    public EmployeeServiceTests()
    {
        _sut = new EmployeeService(_employeeRepoMock.Object, _userServiceMock.Object,
            _loggerMock.Object, _mediaServiceMock.Object, _daDataMock.Object);
    }

    private static User CreateEmployeeUser(bool withProfile = true)
    {
        var user = User.Create("employer@test.com", "Employer", "Password123!", Role.Employee).Value!;
        if (withProfile)
        {
            var info = new EmployeeInfo { INN = "1234567890", Email = "company@test.com" };
            var profile = EmployeeProfile.Create(user.Id, "Company", "Desc", "IT", info, null);
            user.SetEmployeeProfile(profile);
        }
        return user;
    }

    #region UpdateProfileAsync

    [Fact]
    public async Task UpdateProfileAsync_NewProfile_CreatesProfile()
    {
        var user = CreateEmployeeUser(withProfile: false);
        var request = new EmployeeProfileRequest
        {
            Name = "NewCompany",
            Description = "Описание",
            Activity = "IT-разработка",
            Info = new EmployeeInfo { INN = "9876543210", Email = "hr@new.com" },
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.UpdateProfileAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _employeeRepoMock.Verify(x => x.AddAsync(It.IsAny<EmployeeProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ExistingProfile_Updates()
    {
        var user = CreateEmployeeUser();
        var request = new EmployeeProfileRequest
        {
            Name = "UpdatedCompany",
            Description = "Новое описание",
            Activity = "Финтех",
            Info = new EmployeeInfo { INN = "1111111111", Email = "new@company.com" },
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.UpdateProfileAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _employeeRepoMock.Verify(x => x.UpdateAsync(It.IsAny<EmployeeProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsFailure()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.UpdateProfileAsync(Guid.NewGuid(), new EmployeeProfileRequest
        {
            Name = "N",
            Description = "D",
            Activity = "A"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region VerifyCompanyAsync

    [Fact]
    public async Task VerifyCompanyAsync_WithValidINN_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var findResponse = new FindResponse();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.FindParty("1234567890", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FindResponse>.Success(findResponse));

        var result = await _sut.VerifyCompanyAsync(user.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _employeeRepoMock.Verify(x => x.UpdateAsync(It.IsAny<EmployeeProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyCompanyAsync_UserNotFound_ReturnsFailure404()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.VerifyCompanyAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task VerifyCompanyAsync_EmptyINN_ReturnsFailure400()
    {
        var user = CreateEmployeeUser();
        user.EmployeeProfile!.UpdateInfo(new EmployeeInfo { INN = "", Email = "test@test.com" });

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.VerifyCompanyAsync(user.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    [Fact]
    public async Task VerifyCompanyAsync_DaDataFails_ReturnsFailure()
    {
        var user = CreateEmployeeUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.FindParty(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FindResponse>.Failure(new ErrorDetail("inn", "Not found")));

        var result = await _sut.VerifyCompanyAsync(user.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region UpdateVideosAsync / UpdatePhotosAsync

    [Fact]
    public async Task UpdateVideoAsync_WithValidFiles_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var file = new Mock<IFormFile>();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mediaServiceMock.Setup(x => x.UploadFile(file.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("/files/videos/video.mp4"));

        var result = await _sut.UpdateVideoAsync(user.Id, new[] { file.Object }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateVideoAsync_UserNotFoundOrNoProfile_ReturnsFailure()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.UpdateVideoAsync(Guid.NewGuid(), Array.Empty<IFormFile>(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdatePhotosAsync_WithValidFiles_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var file = new Mock<IFormFile>();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mediaServiceMock.Setup(x => x.UploadFile(file.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("/files/photos/photo.jpg"));

        var result = await _sut.UpdatePhotosAsync(user.Id, new[] { file.Object }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region UpdateInfoAsync

    [Fact]
    public async Task UpdateInfoAsync_WithValidData_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var newInfo = new EmployeeInfo { INN = "0000000000", Email = "new@company.com", Address = "СПб" };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.UpdateInfoAsync(user.Id, newInfo, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _employeeRepoMock.Verify(x => x.UpdateAsync(It.IsAny<EmployeeProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateInfoAsync_UserNotFound_ReturnsFailure404()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.UpdateInfoAsync(Guid.NewGuid(), new EmployeeInfo(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion
}
