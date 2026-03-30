using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Trampline.Application.Services;
using Trampline.Application.Services.Events;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Models.Worker;
using Trampline.Core.Repositories;
using Trampline.Shared.Results;

namespace Trampline.Tests.Services;

public class EventServiceTests
{
    private readonly Mock<ILogger<EventService>> _loggerMock = new();
    private readonly Mock<IEventRepository> _eventRepoMock = new();
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<IUserService> _userServiceMock = new();
    private readonly Mock<IEventApplicationRepository> _appRepoMock = new();
    private readonly Mock<IDaDataService> _daDataMock = new();
    private readonly EventService _sut;

    public EventServiceTests()
    {
        _sut = new EventService(_loggerMock.Object, _eventRepoMock.Object, _employeeRepoMock.Object,
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
        var profile = WorkerProfile.Create(user.Id, "Иван", "Иванов", "Иванович", null, null, null);
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

    #region CreateEventAsync

    [Fact]
    public async Task CreateEventAsync_WithValidData_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var request = new CreateEventRequest
        {
            Title = "Хакатон 2026",
            Description = "Описание хакатона",
            Address = "Москва",
            Format = WorkFormat.Hybrid,
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Success(CreateMockGeoResponse()));

        var result = await _sut.CreateEventAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Хакатон 2026");
        result.Value.Format.Should().Be(WorkFormat.Hybrid);
        _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEventAsync_EmployeeNotFound_ReturnsFailure404()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.CreateEventAsync(Guid.NewGuid(), new CreateEventRequest
        {
            Title = "T",
            Description = "D",
            Address = "A"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task CreateEventAsync_NoEmployeeProfile_ReturnsFailure400()
    {
        var user = User.Create("noprofile@test.com", "NoProfile", "Password123!", Role.Employee).Value!;

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.CreateEventAsync(user.Id, new CreateEventRequest
        {
            Title = "T",
            Description = "D",
            Address = "A"
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    [Fact]
    public async Task CreateEventAsync_GeoFails_StillCreatesEventWithRawAddress()
    {
        var user = CreateEmployeeUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Failure(new ErrorDetail("address", "Not found")));

        var result = await _sut.CreateEventAsync(user.Id, new CreateEventRequest
        {
            Title = "T",
            Description = "D",
            Address = "Invalid"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Address.Should().Be("Invalid");
        result.Value!.GeoLat.Should().Be(55.7558);
        result.Value!.GeoLon.Should().Be(37.6173);
        _eventRepoMock.Verify(x => x.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateEventAsync_WithTags_CreatesAndAssignsTags()
    {
        var user = CreateEmployeeUser();
        var request = new CreateEventRequest
        {
            Title = "Митап",
            Description = "Desc",
            Address = "Addr",
            Tags = new[] { new TagRequest { Name = "Python", Category = "tech", Lvl = 1 } },
        };
        var tags = new List<Tag> { new() { Id = Guid.NewGuid(), Name = "Python", Category = "tech", Lvl = 1 } };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Success(CreateMockGeoResponse()));
        _eventRepoMock.Setup(x => x.GetOrCreateTagsAsync(It.IsAny<IEnumerable<Tag>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tags);

        var result = await _sut.CreateEventAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Tags.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateEventAsync_WithSalaryAndDates_SetsAllFields()
    {
        var user = CreateEmployeeUser();
        var startDate = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(2026, 6, 16, 18, 0, 0, DateTimeKind.Utc);
        var request = new CreateEventRequest
        {
            Title = "CTF",
            Description = "Desc",
            Address = "Addr",
            SalaryFrom = 0,
            SalaryTo = 0,
            StartDate = startDate,
            EndedAt = endDate,
        };

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _daDataMock.Setup(x => x.GetGeoByAddress(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<AddressResponse>.Success(CreateMockGeoResponse()));

        var result = await _sut.CreateEventAsync(user.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.StartDate.Should().Be(startDate);
        result.Value.EndedAt.Should().Be(endDate);
    }

    #endregion

    #region ApplicationEventAsync

    [Fact]
    public async Task ApplicationEventAsync_WithValidData_ReturnsSuccess()
    {
        var worker = CreateWorkerUser();
        var evt = Event.Create(Guid.NewGuid(), Guid.NewGuid(), "Event", "Desc", WorkFormat.Remote);

        _userServiceMock.Setup(x => x.GetByIdAsync(worker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(worker);
        _eventRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        var request = new EventApplicationRequest
        {
            EventId = evt.Id,
            CoverLetter = new string('A', 60),
        };

        var result = await _sut.ApplicationEventAsync(worker.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _appRepoMock.Verify(x => x.AddAsync(It.IsAny<EventApplication>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApplicationEventAsync_UserNotFound_ReturnsFailure404()
    {
        _userServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.ApplicationEventAsync(Guid.NewGuid(), new EventApplicationRequest
        {
            EventId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task ApplicationEventAsync_NoWorkerProfile_ReturnsFailure400()
    {
        var user = User.Create("user@test.com", "User", "Password123!", Role.Worker).Value!;

        _userServiceMock.Setup(x => x.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await _sut.ApplicationEventAsync(user.Id, new EventApplicationRequest
        {
            EventId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 400);
    }

    [Fact]
    public async Task ApplicationEventAsync_EventNotFound_ReturnsFailure404()
    {
        var worker = CreateWorkerUser();

        _userServiceMock.Setup(x => x.GetByIdAsync(worker.Id, It.IsAny<CancellationToken>())).ReturnsAsync(worker);
        _eventRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var result = await _sut.ApplicationEventAsync(worker.Id, new EventApplicationRequest
        {
            EventId = Guid.NewGuid(),
            CoverLetter = new string('A', 60)
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    #endregion

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_ExistingEvent_ReturnsSuccess()
    {
        var evt = Event.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        var result = await _sut.GetByIdAsync(evt.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Title");
    }

    [Fact]
    public async Task GetByIdAsync_WithUserId_AddsView()
    {
        var evt = Event.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);
        var viewerId = Guid.NewGuid();

        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        await _sut.GetByIdAsync(evt.Id, CancellationToken.None, viewerId);

        _eventRepoMock.Verify(x => x.UpdateAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithAlreadyViewedUser_DoesNotAddDuplicateView()
    {
        var evt = Event.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);
        var viewerId = Guid.NewGuid();
        evt.AddViews(viewerId);

        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        await _sut.GetByIdAsync(evt.Id, CancellationToken.None, viewerId);

        _eventRepoMock.Verify(x => x.UpdateAsync(evt, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsFailure404()
    {
        _eventRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

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
        var evt = Event.Create(Guid.NewGuid(), userId, "Title", "Desc", WorkFormat.Remote);
        var application = new EventApplication
        {
            Id = Guid.NewGuid(),
            EventId = evt.Id,
            Status = ApplicationStatus.Pending,
        };

        _appRepoMock.Setup(x => x.GetByIdAsync(application.Id, It.IsAny<CancellationToken>())).ReturnsAsync(application);
        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        var result = await _sut.UpdateApplicationStatusAsync(userId, application.Id, ApplicationStatus.Invited, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _appRepoMock.Verify(x => x.UpdateAsync(application, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_ApplicationNotFound_ReturnsFailure404()
    {
        _appRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventApplication?)null);

        var result = await _sut.UpdateApplicationStatusAsync(Guid.NewGuid(), Guid.NewGuid(), ApplicationStatus.Invited, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_EventNotFound_ReturnsFailure404()
    {
        var application = new EventApplication { Id = Guid.NewGuid(), EventId = Guid.NewGuid() };

        _appRepoMock.Setup(x => x.GetByIdAsync(application.Id, It.IsAny<CancellationToken>())).ReturnsAsync(application);
        _eventRepoMock.Setup(x => x.GetByIdAsync(application.EventId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var result = await _sut.UpdateApplicationStatusAsync(Guid.NewGuid(), application.Id, ApplicationStatus.Invited, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_NotOwner_ReturnsFailure403()
    {
        var ownerId = Guid.NewGuid();
        var evt = Event.Create(Guid.NewGuid(), ownerId, "Title", "Desc", WorkFormat.Remote);
        var application = new EventApplication { Id = Guid.NewGuid(), EventId = evt.Id };

        _appRepoMock.Setup(x => x.GetByIdAsync(application.Id, It.IsAny<CancellationToken>())).ReturnsAsync(application);
        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);

        var result = await _sut.UpdateApplicationStatusAsync(Guid.NewGuid(), application.Id, ApplicationStatus.Invited, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    #endregion

    #region GetApplicationsAsync

    [Fact]
    public async Task GetApplicationsAsync_ValidOwner_ReturnsApplications()
    {
        var user = CreateEmployeeUser();
        var evt = Event.Create(user.EmployeeProfile!.Id, user.Id, "Title", "Desc", WorkFormat.Remote);

        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _employeeRepoMock.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.EmployeeProfile);

        var result = await _sut.GetApplicationsAsync(user.Id, evt.Id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetApplicationsAsync_EventNotFound_ReturnsFailure404()
    {
        _eventRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var result = await _sut.GetApplicationsAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task GetApplicationsAsync_NotOwner_ReturnsFailure403()
    {
        var evt = Event.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);
        var otherProfile = EmployeeProfile.Create(Guid.NewGuid(), "Other", "D", "IT", new EmployeeInfo(), null);

        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _employeeRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherProfile);

        var result = await _sut.GetApplicationsAsync(Guid.NewGuid(), evt.Id, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ValidOwner_ReturnsSuccess()
    {
        var user = CreateEmployeeUser();
        var evt = Event.Create(user.EmployeeProfile!.Id, user.Id, "Title", "Desc", WorkFormat.Remote);

        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _employeeRepoMock.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.EmployeeProfile);

        var request = new UpdateEventRequest
        {
            Title = "Updated Title",
            Description = "Updated Desc",
            Address = "New Addr",
            City = "New City",
            Country = "Russia",
            IsPublished = true,
            Tags = Array.Empty<TagRequest>(),
        };

        var result = await _sut.UpdateAsync(user.Id, evt.Id, request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateAsync_EventNotFound_ReturnsFailure404()
    {
        _eventRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        var result = await _sut.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateEventRequest
        {
            Tags = Array.Empty<TagRequest>()
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 404);
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ReturnsFailure403()
    {
        var user = CreateEmployeeUser();
        var otherProfile = EmployeeProfile.Create(Guid.NewGuid(), "Other", "D", "IT", new EmployeeInfo(), null);
        var evt = Event.Create(otherProfile.Id, Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        _eventRepoMock.Setup(x => x.GetByIdAsync(evt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(evt);
        _employeeRepoMock.Setup(x => x.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user.EmployeeProfile);

        var result = await _sut.UpdateAsync(user.Id, evt.Id, new UpdateEventRequest
        {
            Tags = Array.Empty<TagRequest>()
        }, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Code == 403);
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingEvent_SoftDeletes()
    {
        var evt = Event.Create(Guid.NewGuid(), Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        _eventRepoMock.Setup(x => x.GetByEmployeeAsync(evt.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(evt);

        await _sut.DeleteAsync(evt.Id, Guid.NewGuid(), CancellationToken.None);

        evt.DeletedAt.Should().NotBeNull();
        evt.IsActive.Should().BeFalse();
        _eventRepoMock.Verify(x => x.UpdateAsync(evt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExisting_DoesNothing()
    {
        _eventRepoMock.Setup(x => x.GetByEmployeeAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Event?)null);

        await _sut.DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        _eventRepoMock.Verify(x => x.UpdateAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region GetAllAsync / AddAsync

    [Fact]
    public async Task GetAllAsync_ReturnsAllEvents()
    {
        var events = new List<Event>
        {
            Event.Create(Guid.NewGuid(), Guid.NewGuid(), "E1", "D1", WorkFormat.Remote),
            Event.Create(Guid.NewGuid(), Guid.NewGuid(), "E2", "D2", WorkFormat.Hybrid),
        };

        _eventRepoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(events);

        var result = await _sut.GetAllAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    #endregion
}
