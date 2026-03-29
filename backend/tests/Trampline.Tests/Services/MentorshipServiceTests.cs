using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Trampline.Application.Services;
using Trampline.Application.Services.Mentorships;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;

namespace Trampline.Tests.Services;

public class MentorshipServiceTests
{
    private readonly Mock<ILogger<MentorshipService>> _loggerMock = new();
    private readonly Mock<IMentorshipRepository> _mentorshipRepoMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IMentorshipApplicationRepository> _appRepoMock = new();
    private readonly Mock<IDaDataService> _daDataMock = new();
    private readonly MentorshipService _sut;

    public MentorshipServiceTests()
    {
        _sut = new MentorshipService(_loggerMock.Object, _mentorshipRepoMock.Object, _employeeRepoMock.Object,
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

    #region CreateMentorship

    [Fact]
    public async Task CreateMentorship_WithValidData_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var request = new CreateMentorshipRequest
        {
            Title = "React Mentorship",
            Description = "Описание менторства",
            Address = "Москва, ул. Ленина",
            Format = WorkFormat.Remote,
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Success(CreateMockGeoResponse()));

        var result = await _sut.CreateMentorshipAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("React Mentorship");
        _mentorshipRepoMock.Verify(x => x.AddAsync(It.IsAny<Mentorship>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMentorship_EmployeeNotFound_ReturnsFailure404()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.CreateMentorshipAsync(Guid.NewGuid(), new CreateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Address = "A"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task CreateMentorship_NoEmployeeProfile_ReturnsFailure400()
    {
        var user = User.Create("noprofile@test.com", "NoProfile", "Password123!", Role.Employee).Value!;

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.CreateMentorshipAsync(user.Id, new CreateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Address = "A"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    [Fact]
    public async Task CreateMentorship_GeoFails_StillCreatesWithRawAddress()
    {
        var user = CreateEmployeeUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Failure(new ErrorDetail("address", "Not found")));

        var result = await _sut.CreateMentorshipAsync(user.Id, new CreateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Address = "Invalid"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Address.Should().Be("Invalid");
        result.Value!.GeoLat.Should().Be(55.7558);
        result.Value!.GeoLon.Should().Be(37.6173);
        _mentorshipRepoMock.Verify(x => x.AddAsync(It.IsAny<Mentorship>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMentorship_WithTags_CreatesTagsAndAssigns()
    {
        var user = CreateEmployeeUser();
        var request = new CreateMentorshipRequest
        {
            Title = "Dev",
            Description = "Desc",
            Address = "Addr",
            Tags = new[] { new TagRequest { Name = "React", Category = "tech", Lvl = 1 } },
        };
        var tags = new List<Tag> { new() { Id = Guid.NewGuid(), Name = "React", Category = "tech", Lvl = 1 } };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Success(CreateMockGeoResponse()));
        _mentorshipRepoMock.Setup(x => x.GetOrCreateTagsAsync(It.IsAny<IEnumerable<Tag>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        var result = await _sut.CreateMentorshipAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Tags.Should().HaveCount(1);
    }

    #endregion

    #region ApplicationMentorshipAsync

    [Fact]
    public async Task ApplicationMentorshipAsync_WithValidData_ReturnsSuccess()
    {
        var worker = CreateWorkerUser();
        var mentorship = Mentorship.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        _userServiceMock.Setup(x => x.GetByIdAsync(worker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(worker);
        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(mentorship);

        var request = new MentorshipApplicationRequest
        {
            MentorshipId = mentorship.Id,
            CoverLetter = new string('A', 60),
        };

        var result = await _sut.ApplicationMentorshipAsync(worker.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _appRepoMock.Verify(x => x.AddAsync(It.IsAny<MentorshipApplication>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplicationMentorshipAsync_UserNotFound_ReturnsFailure()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.ApplicationMentorshipAsync(Guid.NewGuid(), new MentorshipApplicationRequest
        {
            MentorshipId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task ApplicationMentorshipAsync_NoWorkerProfile_ReturnsFailure400()
    {
        var user = User.Create("user@test.com", "User", "Password123!", Role.Worker).Value!;

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.ApplicationMentorshipAsync(user.Id, new MentorshipApplicationRequest
        {
            MentorshipId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    [Fact]
    public async Task ApplicationMentorshipAsync_MentorshipNotFound_ReturnsFailure404()
    {
        var worker = CreateWorkerUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(worker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(worker);
        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mentorship?)null);

        var result = await _sut.ApplicationMentorshipAsync(worker.Id, new MentorshipApplicationRequest
        {
            MentorshipId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ValidOwner_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var mentorship = Mentorship.Create(user.EmployeeProfile!.Id, user.Id, "Title", "Desc", WorkFormat.Remote);

        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(mentorship.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mentorship);
        _employeeRepoMock.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.EmployeeProfile);

        var request = new UpdateMentorshipRequest
        {
            Title = "Updated",
            Description = "Updated Desc",
            Address = "New Address",
            City = "New City",
            Country = "Russia",
            IsActive = true,
            Tags = Array.Empty<TagRequest>(),
        };

        var result = await _sut.UpdateAsync(user.Id, mentorship.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ReturnsFailure403()
    {
        var user = CreateEmployeeUser();
        var otherProfile = EmployeeProfile.Create(Guid.NewGuid(), "Other", "D", "IT", new EmployeeInfo(), null);
        var mentorship = Mentorship.Create(otherProfile.Id, Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(mentorship.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mentorship);
        _employeeRepoMock.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.EmployeeProfile);

        var result = await _sut.UpdateAsync(user.Id, mentorship.Id, new UpdateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Tags = Array.Empty<TagRequest>()
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    [Fact]
    public async Task UpdateAsync_MentorshipNotFound_ReturnsFailure404()
    {
        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mentorship?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Tags = Array.Empty<TagRequest>()
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingMentorship_SoftDeletes()
    {
        var mentorship = Mentorship.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        _mentorshipRepoMock.Setup(x => x.GetByEmployeeAsync(mentorship.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mentorship);

        await _sut.DeleteAsync(mentorship.Id, Guid.NewGuid(), CancellationToken.None);

        mentorship.DeletedAt.Should().NotBeNull();
        mentorship.IsActive.Should().BeFalse();
        _mentorshipRepoMock.Verify(x => x.UpdateAsync(mentorship, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingMentorship_DoesNothing()
    {
        _mentorshipRepoMock.Setup(x => x.GetByEmployeeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mentorship?)null);

        await _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        _mentorshipRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Mentorship>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingMentorship_ReturnsSuccess()
    {
        var mentorship = Mentorship.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(mentorship.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mentorship);

        var result = await _sut.GetByIdAsync(mentorship.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Title");
    }

    [Fact]
    public async Task GetByIdAsync_WithUserId_AddsView()
    {
        var mentorship = Mentorship.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);
        var viewerId = Guid.NewGuid();

        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(mentorship.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mentorship);

        await _sut.GetByIdAsync(mentorship.Id, CancellationToken.None, viewerId);

        _mentorshipRepoMock.Verify(x => x.UpdateAsync(mentorship, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingMentorship_ReturnsFailure404()
    {
        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mentorship?)null);

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
        var mentorship = Mentorship.Create(Guid.NewGuid(), userId, "Title", "Desc", WorkFormat.Remote);
        var application = new MentorshipApplication
        {
            Id = Guid.NewGuid(),
            MentorshipId = mentorship.Id,
            Status = ApplicationStatus.Pending,
        };

        _appRepoMock.Setup(x => x.GetByIdAsync(application.Id, It.IsAny<CancellationToken>())).ReturnsAsync(application);
        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(mentorship.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mentorship);

        var result = await _sut.UpdateApplicationStatusAsync(userId, application.Id, ApplicationStatus.Invited, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_ApplicationNotFound_ReturnsFailure404()
    {
        _appRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MentorshipApplication?)null);

        var result = await _sut.UpdateApplicationStatusAsync(Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Invited, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_NotOwner_ReturnsFailure403()
    {
        var ownerId = Guid.NewGuid();
        var notOwnerId = Guid.NewGuid();
        var mentorship = Mentorship.Create(Guid.NewGuid(), ownerId, "Title", "Desc", WorkFormat.Remote);
        var application = new MentorshipApplication { Id = Guid.NewGuid(), MentorshipId = mentorship.Id };

        _appRepoMock.Setup(x => x.GetByIdAsync(application.Id, It.IsAny<CancellationToken>())).ReturnsAsync(application);
        _mentorshipRepoMock.Setup(x => x.GetByIdAsync(mentorship.Id, It.IsAny<CancellationToken>())).ReturnsAsync(mentorship);

        var result = await _sut.UpdateApplicationStatusAsync(notOwnerId, application.Id, ApplicationStatus.Invited, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    #endregion
}
