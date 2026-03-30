using FluentAssertions;
using Trampline.Core.Exceptions;
using Trampline.Core.Models.Employee;

namespace Trampline.Tests.Domain;

public class JobTests
{
    private static Job CreateTestJob() =>
        Job.Create(Guid.NewGuid(), Guid.NewGuid(), "Backend Developer", "Описание вакансии", JobType.Work, WorkFormat.Remote);

    [Fact]
    public void Create_SetsCorrectProperties()
    {
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var job = Job.Create(employeeId, userId, "Frontend Developer", "React разработчик", JobType.Internship, WorkFormat.Hybrid);

        job.Id.Should().NotBeEmpty();
        job.EmployeeId.Should().Be(employeeId);
        job.UserId.Should().Be(userId);
        job.Title.Should().Be("Frontend Developer");
        job.Description.Should().Be("React разработчик");
        job.Type.Should().Be(JobType.Internship);
        job.Format.Should().Be(WorkFormat.Hybrid);
        job.IsActive.Should().BeTrue();
        job.Views.Should().Be(0);
        job.Tags.Should().BeEmpty();
        job.JobApplications.Should().BeEmpty();
        job.SalaryFrom.Should().BeNull();
        job.SalaryTo.Should().BeNull();
    }

    [Fact]
    public void UpdateGeo_WithValidCoordinates_UpdatesLocation()
    {
        var job = CreateTestJob();

        job.UpdateGeo("ул. Пушкина д.10", "Москва", "Россия", "ул. Пушкина", 55.7558, 37.6173);

        job.Address.Should().Be("ул. Пушкина д.10");
        job.City.Should().Be("Москва");
        job.Country.Should().Be("Россия");
        job.Street.Should().Be("ул. Пушкина");
        job.GeoLat.Should().Be(55.7558);
        job.GeoLon.Should().Be(37.6173);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    public void UpdateGeo_WithInvalidLatitude_ThrowsDomainException(double lat, double lon)
    {
        var job = CreateTestJob();

        var act = () => job.UpdateGeo("addr", "city", "country", "street", lat, lon);

        act.Should().Throw<DomainException>().WithMessage("*latitude*");
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void UpdateGeo_WithInvalidLongitude_ThrowsDomainException(double lat, double lon)
    {
        var job = CreateTestJob();

        var act = () => job.UpdateGeo("addr", "city", "country", "street", lat, lon);

        act.Should().Throw<DomainException>().WithMessage("*longitude*");
    }

    [Fact]
    public void UpdateGeo_WithBoundaryCoordinates_Succeeds()
    {
        var job = CreateTestJob();

        job.UpdateGeo("addr", "city", "country", "street", 90, 180);

        job.GeoLat.Should().Be(90);
        job.GeoLon.Should().Be(180);
    }

    [Fact]
    public void UpdateSalary_SetsSalaryRange()
    {
        var job = CreateTestJob();

        job.UpdateSalary(50000, 100000);

        job.SalaryFrom.Should().Be(50000);
        job.SalaryTo.Should().Be(100000);
    }

    [Fact]
    public void UpdateSalary_WithNullValues_ClearsSalary()
    {
        var job = CreateTestJob();
        job.UpdateSalary(50000, 100000);

        job.UpdateSalary(null, null);

        job.SalaryFrom.Should().BeNull();
        job.SalaryTo.Should().BeNull();
    }

    [Fact]
    public void AddViews_IncreasesViewCount()
    {
        var job = CreateTestJob();
        var viewerId = Guid.NewGuid();

        job.AddViews(viewerId);

        job.Views.Should().Be(1);
        job.UserViews.Should().Contain(viewerId);
    }

    [Fact]
    public void AddViews_SameUserTwice_DoesNotDuplicate()
    {
        var job = CreateTestJob();
        var viewerId = Guid.NewGuid();

        job.AddViews(viewerId);
        job.AddViews(viewerId);

        job.Views.Should().Be(1);
    }

    [Fact]
    public void AddViews_ByJobOwner_DoesNotCount()
    {
        var employeeId = Guid.NewGuid();
        var job = Job.Create(employeeId, Guid.NewGuid(), "Title", "Desc", JobType.Work, WorkFormat.Remote);

        job.AddViews(employeeId);

        job.Views.Should().Be(0);
    }

    [Fact]
    public void AddViews_MultipleUsers_CountsAll()
    {
        var job = CreateTestJob();

        job.AddViews(Guid.NewGuid());
        job.AddViews(Guid.NewGuid());
        job.AddViews(Guid.NewGuid());

        job.Views.Should().Be(3);
    }

    [Fact]
    public void AddTag_ByString_AddsTag()
    {
        var job = CreateTestJob();

        job.AddTag("C#");

        job.Tags.Should().HaveCount(1);
        job.Tags.First().Name.Should().Be("C#");
    }

    [Fact]
    public void AddTag_EmptyOrWhitespace_DoesNotAdd()
    {
        var job = CreateTestJob();

        job.AddTag("");
        job.AddTag("   ");

        job.Tags.Should().BeEmpty();
    }

    [Fact]
    public void AddTag_DuplicateName_DoesNotAdd()
    {
        var job = CreateTestJob();

        job.AddTag("Python");
        job.AddTag("python");
        job.AddTag("PYTHON");

        job.Tags.Should().HaveCount(1);
    }

    [Fact]
    public void AddTag_TrimsWhitespace()
    {
        var job = CreateTestJob();

        job.AddTag("  React  ");

        job.Tags.First().Name.Should().Be("React");
    }

    [Fact]
    public void AddTag_ByTagObjects_AddsUniqueTags()
    {
        var job = CreateTestJob();
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "Go" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "Rust" };
        var tag3 = new Tag { Id = Guid.NewGuid(), Name = "go" };

        job.AddTag(tag1, tag2, tag3);

        job.Tags.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateTags_ReplacesAllTags()
    {
        var job = CreateTestJob();
        job.AddTag("OldTag1");
        job.AddTag("OldTag2");

        var newTag = new Tag { Id = Guid.NewGuid(), Name = "NewTag" };
        job.UpdateTags(newTag);

        job.Tags.Should().HaveCount(1);
        job.Tags.First().Name.Should().Be("NewTag");
    }

    [Fact]
    public void UpdateEndedAt_WithValue_UpdatesEndDate()
    {
        var job = CreateTestJob();
        var newDate = DateTime.UtcNow.AddMonths(3);

        job.UpdateEndedAt(newDate);

        job.EndedAt.Should().Be(newDate);
    }

    [Fact]
    public void UpdateEndedAt_WithNull_DoesNotChange()
    {
        var job = CreateTestJob();
        var original = job.EndedAt;

        job.UpdateEndedAt(null);

        job.EndedAt.Should().Be(original);
    }

    [Fact]
    public void SoftDelete_SetsDeletedAtAndDeactivates()
    {
        var job = CreateTestJob();

        job.SoftDelete();

        job.DeletedAt.Should().NotBeNull();
        job.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        job.IsActive.Should().BeFalse();
    }

    [Fact]
    public void SetActive_ChangesActiveStatus()
    {
        var job = CreateTestJob();

        job.SetActive(false);
        job.IsActive.Should().BeFalse();

        job.SetActive(true);
        job.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_UpdatesAllFields()
    {
        var job = CreateTestJob();

        job.Update("New Title", "New Description", "New Address", "New City", "New Country");

        job.Title.Should().Be("New Title");
        job.Description.Should().Be("New Description");
        job.Address.Should().Be("New Address");
        job.City.Should().Be("New City");
        job.Country.Should().Be("New Country");
        job.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
