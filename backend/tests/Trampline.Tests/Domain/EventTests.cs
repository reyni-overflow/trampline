using FluentAssertions;
using Trampline.Core.Exceptions;
using Trampline.Core.Models.Employee;

namespace Trampline.Tests.Domain;

public class EventTests
{
    private static Event CreateTestEvent() =>
        Event.Create(Guid.NewGuid(), Guid.NewGuid(), "Хакатон 2026", "Описание хакатона", WorkFormat.Hybrid);

    [Fact]
    public void Create_SetsCorrectProperties()
    {
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var evt = Event.Create(employeeId, userId, "Митап", "Описание", WorkFormat.Remote);

        evt.Id.Should().NotBeEmpty();
        evt.EmployeeId.Should().Be(employeeId);
        evt.UserId.Should().Be(userId);
        evt.Title.Should().Be("Митап");
        evt.Description.Should().Be("Описание");
        evt.Format.Should().Be(WorkFormat.Remote);
        evt.IsActive.Should().BeTrue();
        evt.Views.Should().Be(0);
        evt.Tags.Should().BeEmpty();
        evt.EventApplications.Should().BeEmpty();
        evt.StartDate.Should().BeNull();
    }

    [Fact]
    public void UpdateGeo_WithValidCoordinates_UpdatesLocation()
    {
        var evt = CreateTestEvent();

        evt.UpdateGeo("addr", "СПб", "Россия", "ул. Ленина", 59.9343, 30.3351);

        evt.City.Should().Be("СПб");
        evt.Country.Should().Be("Россия");
        evt.GeoLat.Should().Be(59.9343);
        evt.GeoLon.Should().Be(30.3351);
    }

    [Fact]
    public void UpdateGeo_WithInvalidLatitude_ThrowsDomainException()
    {
        var evt = CreateTestEvent();

        var act = () => evt.UpdateGeo("addr", "city", "country", "street", 91, 0);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateGeo_WithInvalidLongitude_ThrowsDomainException()
    {
        var evt = CreateTestEvent();

        var act = () => evt.UpdateGeo("addr", "city", "country", "street", 0, 181);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddViews_TracksUniqueViewers()
    {
        var evt = CreateTestEvent();
        var viewer1 = Guid.NewGuid();
        var viewer2 = Guid.NewGuid();

        evt.AddViews(viewer1);
        evt.AddViews(viewer2);
        evt.AddViews(viewer1);

        evt.Views.Should().Be(2);
    }

    [Fact]
    public void AddViews_ByEventOwner_DoesNotCount()
    {
        var employeeId = Guid.NewGuid();
        var evt = Event.Create(employeeId, Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        evt.AddViews(employeeId);

        evt.Views.Should().Be(0);
    }

    [Fact]
    public void UpdateSalary_SetsSalaryRange()
    {
        var evt = CreateTestEvent();

        evt.UpdateSalary(1000, 5000);

        evt.SalaryFrom.Should().Be(1000);
        evt.SalaryTo.Should().Be(5000);
    }

    [Fact]
    public void AddTag_ByString_HandlesCorrectly()
    {
        var evt = CreateTestEvent();

        evt.AddTag("Python");
        evt.AddTag("python");
        evt.AddTag("");

        evt.Tags.Should().HaveCount(1);
    }

    [Fact]
    public void UpdateTags_ReplacesAllTags()
    {
        var evt = CreateTestEvent();
        evt.AddTag("Old");

        evt.UpdateTags(new Tag { Id = Guid.NewGuid(), Name = "New1" }, new Tag { Id = Guid.NewGuid(), Name = "New2" });

        evt.Tags.Should().HaveCount(2);
        evt.Tags.Select(t => t.Name).Should().Contain("New1").And.Contain("New2");
    }

    [Fact]
    public void UpdateStartDate_SetsStartDate()
    {
        var evt = CreateTestEvent();
        var date = new DateTime(2026, 6, 15, 10, 0, 0, DateTimeKind.Utc);

        evt.UpdateStartDate(date);

        evt.StartDate.Should().Be(date);
    }

    [Fact]
    public void SoftDelete_DeactivatesAndSetsDeletedAt()
    {
        var evt = CreateTestEvent();

        evt.SoftDelete();

        evt.IsActive.Should().BeFalse();
        evt.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_UpdatesAllFields()
    {
        var evt = CreateTestEvent();

        evt.Update("Новое название", "Новое описание", "Новый адрес", "Казань", "Россия");

        evt.Title.Should().Be("Новое название");
        evt.Description.Should().Be("Новое описание");
        evt.Address.Should().Be("Новый адрес");
        evt.City.Should().Be("Казань");
        evt.Country.Should().Be("Россия");
    }
}
