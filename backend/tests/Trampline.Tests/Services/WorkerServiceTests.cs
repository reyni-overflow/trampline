using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Trampline.Application.Services;
using Trampline.Application.Services.IO;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Models.Worker;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;

namespace Trampline.Tests.Services;

public class WorkerServiceTests
{
    private readonly Mock<ILogger<WorkerService>> _loggerMock = new();
    private readonly Mock<IWorkerRepository> _workerRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IMediaService> _mediaServiceMock = new();
    private readonly Mock<IEventApplicationRepository> _eventAppRepoMock = new();
    private readonly Mock<IMentorshipApplicationRepository> _mentorshipAppRepoMock = new();
    private readonly WorkerService _sut;

    public WorkerServiceTests()
    {
        _sut = new WorkerService(_loggerMock.Object, _workerRepoMock.Object,
            _userServiceMock.Object, _mediaServiceMock.Object, _eventAppRepoMock.Object, _mentorshipAppRepoMock.Object);
    }

    private static User CreateWorkerUser(bool withProfile = true)
    {
        var user = User.Create("worker@test.com", "Worker", "Password123!", Role.Worker).Value!;
        if (withProfile)
        {
            var profile = WorkerProfile.Create(user.Id, "Иван", "Иванов", "Иванович", null, "О себе", null);
            user.SetWorkerProfile(profile);
        }
        return user;
    }

    #region GetApplicationsAsync

    [Fact]
    public async Task GetApplicationsAsync_WithExistingUser_ReturnsApplications()
    {
        var user = CreateWorkerUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.GetApplicationsAsync(user.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetApplicationsAsync_UserNotFound_ReturnsFailure()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetApplicationsAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion

    #region GetEventApplicationsAsync

    [Fact]
    public async Task GetEventApplicationsAsync_UserNotFound_ReturnsFailure()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetEventApplicationsAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetEventApplicationsAsync_NoWorkerProfile_ReturnsFailure400()
    {
        var user = CreateWorkerUser(withProfile: false);
        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.GetEventApplicationsAsync(user.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    [Fact]
    public async Task GetEventApplicationsAsync_WithProfile_ReturnsApplications()
    {
        var user = CreateWorkerUser();
        var apps = new List<EventApplication>();
        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _eventAppRepoMock.Setup(x => x.GetByWorkerProfileIdAsync(user.WorkerProfile!.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apps);

        var result = await _sut.GetEventApplicationsAsync(user.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region UpdateProfileAsync

    [Fact]
    public async Task UpdateProfileAsync_NewProfile_CreatesAndReturnsSuccess()
    {
        var user = CreateWorkerUser(withProfile: false);
        var request = new WorkerProfileRequest
        {
            Name = "Алексей",
            LastName = "Смирнов",
            Patronymic = "Игоревич",
            About = "О себе",
            Skills = new List<string> { "C#", "SQL" },
            Repos = new List<string> { "https://github.com/user/repo" },
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.UpdateProfileAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _workerRepoMock.Verify(x => x.AddAsync(It.IsAny<WorkerProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_ExistingProfile_UpdatesAndReturnsSuccess()
    {
        var user = CreateWorkerUser();
        var request = new WorkerProfileRequest
        {
            Name = "Обновленное",
            LastName = "Фамилия",
            Patronymic = "Отчество",
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.UpdateProfileAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _workerRepoMock.Verify(x => x.UpdateAsync(It.IsAny<WorkerProfile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsFailure()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.UpdateProfileAsync(Guid.NewGuid(), new WorkerProfileRequest
        {
            Name = "N",
            LastName = "L",
            Patronymic = "P"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfileAsync_WithWorkerInfo_SetsInfo()
    {
        var user = CreateWorkerUser(withProfile: false);
        var request = new WorkerProfileRequest
        {
            Name = "Name",
            LastName = "Last",
            Patronymic = "Pat",
            Info = new WorkerInfo("МГУ", 2, new DateTime(2024, 9, 1), new DateTime(2028, 6, 30)),
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.UpdateProfileAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateProfileAsync_InvalidWorkerInfo_ReturnsFailure()
    {
        var user = CreateWorkerUser(withProfile: false);
        var request = new WorkerProfileRequest
        {
            Name = "Name",
            LastName = "Last",
            Patronymic = "Pat",
            Info = new WorkerInfo("", 0, DateTime.UtcNow, DateTime.UtcNow.AddYears(-1)),
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.UpdateProfileAsync(user.Id, request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region UpdateResumeAsync

    [Fact]
    public async Task UpdateResumeAsync_WithValidFile_ReturnsSuccess()
    {
        var user = CreateWorkerUser();
        var fileMock = new Mock<IFormFile>();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mediaServiceMock.Setup(x => x.UploadFile(fileMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("/files/resumes/resume.pdf"));

        var result = await _sut.UpdateResumeAsync(user.Id, fileMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateResumeAsync_UploadFails_ReturnsFailure()
    {
        var user = CreateWorkerUser();
        var fileMock = new Mock<IFormFile>();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mediaServiceMock.Setup(x => x.UploadFile(fileMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure(new ErrorDetail("file", "Invalid")));

        var result = await _sut.UpdateResumeAsync(user.Id, fileMock.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateResumeAsync_NoProfile_ReturnsFailure400()
    {
        var user = CreateWorkerUser(withProfile: false);
        var fileMock = new Mock<IFormFile>();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mediaServiceMock.Setup(x => x.UploadFile(fileMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("/files/resumes/resume.pdf"));

        var result = await _sut.UpdateResumeAsync(user.Id, fileMock.Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    #endregion

    #region UpdateAvatarAsync

    [Fact]
    public async Task UpdateAvatarAsync_WithValidFile_ReturnsSuccess()
    {
        var user = CreateWorkerUser();
        var fileMock = new Mock<IFormFile>();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mediaServiceMock.Setup(x => x.UploadFile(fileMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success("/files/photos/avatar.jpg"));

        var result = await _sut.UpdateAvatarAsync(user.Id, fileMock.Object, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _userServiceMock.Verify(x => x.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAvatarAsync_UserNotFound_ReturnsFailure()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.UpdateAvatarAsync(Guid.NewGuid(), new Mock<IFormFile>().Object, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    #endregion
}
