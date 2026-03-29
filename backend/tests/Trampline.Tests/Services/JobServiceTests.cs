using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Trampline.Application.Services;
using Trampline.Application.Services.Employees;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;

namespace Trampline.Tests.Services;

public class JobServiceTests
{
    private readonly Mock<ILogger<JobService>> _loggerMock = new();
    private readonly Mock<IJobRepository> _jobRepoMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IJobApplicationRepository> _appRepoMock = new();
    private readonly Mock<IDaDataService> _daDataMock = new();
    private readonly JobService _sut;

    public JobServiceTests()
    {
        _sut = new JobService(_loggerMock.Object, _jobRepoMock.Object, _employeeRepoMock.Object,
            _userServiceMock.Object, _appRepoMock.Object, _daDataMock.Object);
    }

    private static User CreateEmployeeUser()
    {
        var user = User.Create("employer@test.com", "Employer", "Password123!", Role.Employee).Value!;
        var info = new EmployeeInfo { INN = "1234567890", Email = "company@test.com" };
        var profile = EmployeeProfile.Create(user.Id, "Company", "Desc", "IT", info, null);
        profile.SetVerified(true);
        user.SetEmployeeProfile(profile);
        return user;
    }

    private static User CreateWorkerUser()
    {
        var user = User.Create("worker@test.com", "Worker", "Password123!", Role.Worker).Value!;
        var profile = Core.Models.Worker.WorkerProfile.Create(user.Id, "Иван", "Иванов", "Иванович", null, null, null);
        user.SetWorkerProfile(profile);
        return user;
    }

    private static AddressResponse CreateMockGeoResponse() => new()
    {
        Address = "г. Москва, ул. Ленина, д.1",
        City = "Москва",
        Country = "Россия",
        Street = "ул. Ленина",
        GeoLat = "55.7558",
        GeoLon = "37.6173"
    };

    #region CreateJob

    [Fact]
    public async Task CreateJob_WithValidData_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var request = new CreateJobRequest
        {
            Title = "Backend Developer",
            Description = "Описание вакансии",
            Address = "Москва, ул. Ленина",
            Type = JobType.Work,
            Format = WorkFormat.Remote,
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Success(CreateMockGeoResponse()));

        var result = await _sut.CreateJob(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Backend Developer");
        _jobRepoMock.Verify(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateJob_EmployeeNotFound_ReturnsFailure404()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.CreateJob(Guid.NewGuid(), new CreateJobRequest
        {
            Title = "T",
            Description = "D",
            Address = "A"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task CreateJob_NoEmployeeProfile_ReturnsFailure400()
    {
        var user = User.Create("noprofile@test.com", "NoProfile", "Password123!", Role.Employee).Value!;

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.CreateJob(user.Id, new CreateJobRequest
        {
            Title = "T",
            Description = "D",
            Address = "A"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    [Fact]
    public async Task CreateJob_GeoFails_StillCreatesJobWithRawAddress()
    {
        var user = CreateEmployeeUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Failure(new ErrorDetail("address", "Not found")));

        var result = await _sut.CreateJob(user.Id, new CreateJobRequest
        {
            Title = "T",
            Description = "D",
            Address = "Invalid"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Address.Should().Be("Invalid");
        result.Value!.GeoLat.Should().Be(55.7558);
        result.Value!.GeoLon.Should().Be(37.6173);
        _jobRepoMock.Verify(x => x.AddAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateJob_WithTags_CreatesTagsAndAssigns()
    {
        var user = CreateEmployeeUser();
        var request = new CreateJobRequest
        {
            Title = "Dev",
            Description = "Desc",
            Address = "Addr",
            Tags = new[] { new TagRequest { Name = "C#", Category = "tech", Lvl = 1 } },
        };
        var tags = new List<Tag> { new() { Id = Guid.NewGuid(), Name = "C#", Category = "tech", Lvl = 1 } };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Success(CreateMockGeoResponse()));
        _jobRepoMock.Setup(x => x.GetOrCreateTagsAsync(It.IsAny<IEnumerable<Tag>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        var result = await _sut.CreateJob(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Tags.Should().HaveCount(1);
    }

    #endregion

    #region ApplicationJobAsync

    [Fact]
    public async Task ApplicationJobAsync_WithValidData_ReturnsSuccess()
    {
        var worker = CreateWorkerUser();
        var job = Job.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", JobType.Work, WorkFormat.Remote);

        _userServiceMock.Setup(x => x.GetByIdAsync(worker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(worker);
        _jobRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(job);

        var request = new JobApplicationRequest
        {
            JobId = job.Id,
            CoverLetter = new string('A', 60),
        };

        var result = await _sut.ApplicationJobAsync(worker.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _appRepoMock.Verify(x => x.AddAsync(It.IsAny<JobApplication>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplicationJobAsync_UserNotFound_ReturnsFailure()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.ApplicationJobAsync(Guid.NewGuid(), new JobApplicationRequest
        {
            JobId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task ApplicationJobAsync_NoWorkerProfile_ReturnsFailure400()
    {
        var user = User.Create("user@test.com", "User", "Password123!", Role.Worker).Value!;

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.ApplicationJobAsync(user.Id, new JobApplicationRequest
        {
            JobId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    [Fact]
    public async Task ApplicationJobAsync_JobNotFound_ReturnsFailure404()
    {
        var worker = CreateWorkerUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(worker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(worker);
        _jobRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        var result = await _sut.ApplicationJobAsync(worker.Id, new JobApplicationRequest
        {
            JobId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingJob_ReturnsSuccess()
    {
        var job = Job.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", JobType.Work, WorkFormat.Remote);

        _jobRepoMock.Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>())).ReturnsAsync(job);

        var result = await _sut.GetByIdAsync(job.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Title");
    }

    [Fact]
    public async Task GetByIdAsync_WithUserId_AddsView()
    {
        var job = Job.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", JobType.Work, WorkFormat.Remote);
        var viewerId = Guid.NewGuid();

        _jobRepoMock.Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>())).ReturnsAsync(job);

        await _sut.GetByIdAsync(job.Id, CancellationToken.None, viewerId);

        _jobRepoMock.Verify(x => x.UpdateAsync(job, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingJob_ReturnsFailure404()
    {
        _jobRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion

    #region UpdateApplicationStatusAsync

    [Fact]
    public async Task UpdateApplicationStatusAsync_ValidOwner_ReturnsSuccess()
    {
        var userId = Guid.NewGuid();
        var job = Job.Create(Guid.NewGuid(), userId, "Title", "Desc", JobType.Work, WorkFormat.Remote);
        var application = new JobApplication
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            Status = ApplicationStatus.Pending,
        };

        _appRepoMock.Setup(x => x.GetByIdAsync(application.Id, It.IsAny<CancellationToken>())).ReturnsAsync(application);
        _jobRepoMock.Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>())).ReturnsAsync(job);

        var result = await _sut.UpdateApplicationStatusAsync(userId, application.Id, ApplicationStatus.Invited, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_ApplicationNotFound_ReturnsFailure404()
    {
        _appRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((JobApplication?)null);

        var result = await _sut.UpdateApplicationStatusAsync(Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Invited, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_NotOwner_ReturnsFailure403()
    {
        var ownerId = Guid.NewGuid();
        var notOwnerId = Guid.NewGuid();
        var job = Job.Create(Guid.NewGuid(), ownerId, "Title", "Desc", JobType.Work, WorkFormat.Remote);
        var application = new JobApplication { Id = Guid.NewGuid(), JobId = job.Id };

        _appRepoMock.Setup(x => x.GetByIdAsync(application.Id, It.IsAny<CancellationToken>())).ReturnsAsync(application);
        _jobRepoMock.Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>())).ReturnsAsync(job);

        var result = await _sut.UpdateApplicationStatusAsync(notOwnerId, application.Id, ApplicationStatus.Invited, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ValidOwner_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var job = Job.Create(user.EmployeeProfile!.Id, user.Id, "Title", "Desc", JobType.Work, WorkFormat.Remote);

        _jobRepoMock.Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>())).ReturnsAsync(job);
        _employeeRepoMock.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.EmployeeProfile);

        var request = new UpdateJobRequest
        {
            Title = "Updated",
            Description = "Updated Desc",
            Address = "New Address",
            City = "New City",
            Country = "Russia",
            IsActive = true,
            Tags = Array.Empty<TagRequest>(),
        };

        var result = await _sut.UpdateAsync(user.Id, job.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ReturnsFailure403()
    {
        var user = CreateEmployeeUser();
        var otherProfile = EmployeeProfile.Create(Guid.NewGuid(), "Other", "D", "IT", new EmployeeInfo(), null);
        var job = Job.Create(otherProfile.Id, Guid.NewGuid(), "Title", "Desc", JobType.Work, WorkFormat.Remote);

        _jobRepoMock.Setup(x => x.GetByIdAsync(job.Id, It.IsAny<CancellationToken>())).ReturnsAsync(job);
        _employeeRepoMock.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.EmployeeProfile);

        var result = await _sut.UpdateAsync(user.Id, job.Id, new UpdateJobRequest
        {
            Title = "T",
            Description = "D",
            Tags = Array.Empty<TagRequest>()
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingJob_SoftDeletes()
    {
        var job = Job.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", JobType.Work, WorkFormat.Remote);

        _jobRepoMock.Setup(x => x.GetByEmployeeAsync(job.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        await _sut.DeleteAsync(job.Id, Guid.NewGuid(), CancellationToken.None);

        job.DeletedAt.Should().NotBeNull();
        job.IsActive.Should().BeFalse();
        _jobRepoMock.Verify(x => x.UpdateAsync(job, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingJob_DoesNothing()
    {
        _jobRepoMock.Setup(x => x.GetByEmployeeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        await _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        _jobRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
