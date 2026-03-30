using FluentAssertions;
using Trampline.Core.Exceptions;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;

namespace Trampline.Tests.Domain;

public class MentorshipTests
{
    private static Mentorship CreateTestMentorship() =>
        Mentorship.Create(Guid.NewGuid(), Guid.NewGuid(), "Fullstack Mentorship", "Описание менторства", WorkFormat.Remote);

    #region Create

    [Fact]
    public void Create_SetsCorrectProperties()
    {
        var employeeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var mentorship = Mentorship.Create(employeeId, userId, "React Mentorship", "React разработка", WorkFormat.Hybrid);

        mentorship.Id.Should().NotBeEmpty();
        mentorship.EmployeeId.Should().Be(employeeId);
        mentorship.UserId.Should().Be(userId);
        mentorship.Title.Should().Be("React Mentorship");
        mentorship.Description.Should().Be("React разработка");
        mentorship.Format.Should().Be(WorkFormat.Hybrid);
        mentorship.IsActive.Should().BeTrue();
        mentorship.Views.Should().Be(0);
        mentorship.Tags.Should().BeEmpty();
        mentorship.MentorshipApplications.Should().BeEmpty();
        mentorship.SalaryFrom.Should().BeNull();
        mentorship.SalaryTo.Should().BeNull();
        mentorship.MaxParticipants.Should().BeNull();
        mentorship.Duration.Should().BeNull();
        mentorship.StartDate.Should().BeNull();
    }

    #endregion

    #region UpdateGeo

    [Fact]
    public void UpdateGeo_WithValidCoordinates_UpdatesLocation()
    {
        var mentorship = CreateTestMentorship();

        mentorship.UpdateGeo("ул. Пушкина д.10", "Москва", "Россия", "ул. Пушкина", 55.7558, 37.6173);

        mentorship.Address.Should().Be("ул. Пушкина д.10");
        mentorship.City.Should().Be("Москва");
        mentorship.Country.Should().Be("Россия");
        mentorship.Street.Should().Be("ул. Пушкина");
        mentorship.GeoLat.Should().Be(55.7558);
        mentorship.GeoLon.Should().Be(37.6173);
    }

    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    public void UpdateGeo_WithInvalidLatitude_ThrowsDomainException(double lat, double lon)
    {
        var mentorship = CreateTestMentorship();

        var act = () => mentorship.UpdateGeo("addr", "city", "country", "street", lat, lon);

        act.Should().Throw<DomainException>().WithMessage("*latitude*");
    }

    [Theory]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void UpdateGeo_WithInvalidLongitude_ThrowsDomainException(double lat, double lon)
    {
        var mentorship = CreateTestMentorship();

        var act = () => mentorship.UpdateGeo("addr", "city", "country", "street", lat, lon);

        act.Should().Throw<DomainException>().WithMessage("*longitude*");
    }

    [Fact]
    public void UpdateGeo_WithBoundaryCoordinates_Succeeds()
    {
        var mentorship = CreateTestMentorship();

        mentorship.UpdateGeo("addr", "city", "country", "street", 90, 180);

        mentorship.GeoLat.Should().Be(90);
        mentorship.GeoLon.Should().Be(180);
    }

    #endregion

    #region UpdateSalary

    [Fact]
    public void UpdateSalary_SetsSalaryRange()
    {
        var mentorship = CreateTestMentorship();

        mentorship.UpdateSalary(50000, 100000);

        mentorship.SalaryFrom.Should().Be(50000);
        mentorship.SalaryTo.Should().Be(100000);
    }

    [Fact]
    public void UpdateSalary_WithNullValues_ClearsSalary()
    {
        var mentorship = CreateTestMentorship();
        mentorship.UpdateSalary(50000, 100000);

        mentorship.UpdateSalary(null, null);

        mentorship.SalaryFrom.Should().BeNull();
        mentorship.SalaryTo.Should().BeNull();
    }

    #endregion

    #region AddViews

    [Fact]
    public void AddViews_IncreasesViewCount()
    {
        var mentorship = CreateTestMentorship();
        var viewerId = Guid.NewGuid();

        mentorship.AddViews(viewerId);

        mentorship.Views.Should().Be(1);
        mentorship.UserViews.Should().Contain(viewerId);
    }

    [Fact]
    public void AddViews_SameUserTwice_DoesNotDuplicate()
    {
        var mentorship = CreateTestMentorship();
        var viewerId = Guid.NewGuid();

        mentorship.AddViews(viewerId);
        mentorship.AddViews(viewerId);

        mentorship.Views.Should().Be(1);
    }

    [Fact]
    public void AddViews_ByMentorshipOwner_DoesNotCount()
    {
        var employeeId = Guid.NewGuid();
        var mentorship = Mentorship.Create(employeeId, Guid.NewGuid(), "Title", "Desc", WorkFormat.Remote);

        mentorship.AddViews(employeeId);

        mentorship.Views.Should().Be(0);
    }

    [Fact]
    public void AddViews_MultipleUsers_CountsAll()
    {
        var mentorship = CreateTestMentorship();

        mentorship.AddViews(Guid.NewGuid());
        mentorship.AddViews(Guid.NewGuid());
        mentorship.AddViews(Guid.NewGuid());

        mentorship.Views.Should().Be(3);
    }

    #endregion

    #region Tags

    [Fact]
    public void AddTag_ByString_AddsTag()
    {
        var mentorship = CreateTestMentorship();

        mentorship.AddTag("C#");

        mentorship.Tags.Should().HaveCount(1);
        mentorship.Tags.First().Name.Should().Be("C#");
    }

    [Fact]
    public void AddTag_EmptyOrWhitespace_DoesNotAdd()
    {
        var mentorship = CreateTestMentorship();

        mentorship.AddTag("");
        mentorship.AddTag("   ");

        mentorship.Tags.Should().BeEmpty();
    }

    [Fact]
    public void AddTag_DuplicateName_DoesNotAdd()
    {
        var mentorship = CreateTestMentorship();

        mentorship.AddTag("Python");
        mentorship.AddTag("python");
        mentorship.AddTag("PYTHON");

        mentorship.Tags.Should().HaveCount(1);
    }

    [Fact]
    public void AddTag_TrimsWhitespace()
    {
        var mentorship = CreateTestMentorship();

        mentorship.AddTag("  React  ");

        mentorship.Tags.First().Name.Should().Be("React");
    }

    [Fact]
    public void AddTag_ByTagObjects_AddsUniqueTags()
    {
        var mentorship = CreateTestMentorship();
        var tag1 = new Tag { Id = Guid.NewGuid(), Name = "Go" };
        var tag2 = new Tag { Id = Guid.NewGuid(), Name = "Rust" };
        var tag3 = new Tag { Id = Guid.NewGuid(), Name = "go" };

        mentorship.AddTag(tag1, tag2, tag3);

        mentorship.Tags.Should().HaveCount(2);
    }

    [Fact]
    public void UpdateTags_ReplacesAllTags()
    {
        var mentorship = CreateTestMentorship();
        mentorship.AddTag("OldTag1");
        mentorship.AddTag("OldTag2");

        var newTag = new Tag { Id = Guid.NewGuid(), Name = "NewTag" };
        mentorship.UpdateTags(newTag);

        mentorship.Tags.Should().HaveCount(1);
        mentorship.Tags.First().Name.Should().Be("NewTag");
    }

    #endregion

    #region SoftDelete

    [Fact]
    public void SoftDelete_SetsDeletedAtAndDeactivates()
    {
        var mentorship = CreateTestMentorship();

        mentorship.SoftDelete();

        mentorship.DeletedAt.Should().NotBeNull();
        mentorship.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        mentorship.IsActive.Should().BeFalse();
    }

    #endregion

    #region SetActive

    [Fact]
    public void SetActive_ChangesActiveStatus()
    {
        var mentorship = CreateTestMentorship();

        mentorship.SetActive(false);
        mentorship.IsActive.Should().BeFalse();

        mentorship.SetActive(true);
        mentorship.IsActive.Should().BeTrue();
    }

    #endregion

    #region MaxParticipants, Duration, StartDate

    [Fact]
    public void UpdateMaxParticipants_SetsValue()
    {
        var mentorship = CreateTestMentorship();

        mentorship.UpdateMaxParticipants(25);

        mentorship.MaxParticipants.Should().Be(25);
    }

    [Fact]
    public void UpdateMaxParticipants_WithNull_ClearsValue()
    {
        var mentorship = CreateTestMentorship();
        mentorship.UpdateMaxParticipants(25);

        mentorship.UpdateMaxParticipants(null);

        mentorship.MaxParticipants.Should().BeNull();
    }

    [Fact]
    public void UpdateDuration_SetsValue()
    {
        var mentorship = CreateTestMentorship();

        mentorship.UpdateDuration("3 месяца");

        mentorship.Duration.Should().Be("3 месяца");
    }

    [Fact]
    public void UpdateDuration_WithNull_ClearsValue()
    {
        var mentorship = CreateTestMentorship();
        mentorship.UpdateDuration("3 месяца");

        mentorship.UpdateDuration(null);

        mentorship.Duration.Should().BeNull();
    }

    [Fact]
    public void UpdateStartDate_SetsValue()
    {
        var mentorship = CreateTestMentorship();
        var startDate = DateTime.UtcNow.AddDays(7);

        mentorship.UpdateStartDate(startDate);

        mentorship.StartDate.Should().Be(startDate);
    }

    [Fact]
    public void UpdateStartDate_WithNull_ClearsValue()
    {
        var mentorship = CreateTestMentorship();
        mentorship.UpdateStartDate(DateTime.UtcNow);

        mentorship.UpdateStartDate(null);

        mentorship.StartDate.Should().BeNull();
    }

    #endregion

    #region UpdateEndedAt

    [Fact]
    public void UpdateEndedAt_WithValue_UpdatesEndDate()
    {
        var mentorship = CreateTestMentorship();
        var newDate = DateTime.UtcNow.AddMonths(3);

        mentorship.UpdateEndedAt(newDate);

        mentorship.EndedAt.Should().Be(newDate);
    }

    [Fact]
    public void UpdateEndedAt_WithNull_DoesNotChange()
    {
        var mentorship = CreateTestMentorship();
        var original = mentorship.EndedAt;

        mentorship.UpdateEndedAt(null);

        mentorship.EndedAt.Should().Be(original);
    }

    #endregion

    #region Update

    [Fact]
    public void Update_UpdatesAllFields()
    {
        var mentorship = CreateTestMentorship();

        mentorship.Update("New Title", "New Description", "New Address", "New City", "New Country");

        mentorship.Title.Should().Be("New Title");
        mentorship.Description.Should().Be("New Description");
        mentorship.Address.Should().Be("New Address");
        mentorship.City.Should().Be("New City");
        mentorship.Country.Should().Be("New Country");
        mentorship.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region MentorshipApplication

    [Fact]
    public void MentorshipApplication_Create_WithValidData_Succeeds()
    {
        var workerId = Guid.NewGuid();
        var mentorshipId = Guid.NewGuid();
        var coverLetter = new string('A', 60);

        var application = MentorshipApplication.Create(workerId, mentorshipId, coverLetter);

        application.Id.Should().NotBeEmpty();
        application.WorkerProfileId.Should().Be(workerId);
        application.MentorshipId.Should().Be(mentorshipId);
        application.CoverLetter.Should().Be(coverLetter);
        application.Status.Should().Be(ApplicationStatus.Pending);
        application.IsReadByEmployer.Should().BeFalse();
    }

    [Fact]
    public void MentorshipApplication_Create_WithShortCoverLetter_ThrowsDomainException()
    {
        var act = () => MentorshipApplication.Create(Guid.NewGuid(), Guid.NewGuid(), "Short");

        act.Should().Throw<DomainException>().WithMessage("*50*");
    }

    [Fact]
    public void MentorshipApplication_Create_WithEmptyCoverLetter_ThrowsDomainException()
    {
        var act = () => MentorshipApplication.Create(Guid.NewGuid(), Guid.NewGuid(), "");

        act.Should().Throw<DomainException>().WithMessage("*50*");
    }

    [Fact]
    public void MentorshipApplication_Create_WithWhitespaceCoverLetter_ThrowsDomainException()
    {
        var act = () => MentorshipApplication.Create(Guid.NewGuid(), Guid.NewGuid(), "   ");

        act.Should().Throw<DomainException>().WithMessage("*50*");
    }

    [Fact]
    public void MentorshipApplication_Create_WithExactly50Chars_Succeeds()
    {
        var coverLetter = new string('A', 50);

        var application = MentorshipApplication.Create(Guid.NewGuid(), Guid.NewGuid(), coverLetter);

        application.CoverLetter.Should().Be(coverLetter);
    }

    [Fact]
    public void MentorshipApplication_Create_TrimsCoverLetter()
    {
        var coverLetter = "  " + new string('A', 55) + "  ";

        var application = MentorshipApplication.Create(Guid.NewGuid(), Guid.NewGuid(), coverLetter);

        application.CoverLetter.Should().Be(coverLetter.Trim());
    }

    [Fact]
    public void MentorshipApplication_UpdateStatus_ChangesStatus()
    {
        var application = MentorshipApplication.Create(Guid.NewGuid(), Guid.NewGuid(), new string('A', 60));

        application.UpdateStatus(ApplicationStatus.Invited);

        application.Status.Should().Be(ApplicationStatus.Invited);
        application.IsReadByEmployer.Should().BeTrue();
    }

    [Fact]
    public void MentorshipApplication_UpdateStatus_ToPending_DoesNotMarkRead()
    {
        var application = MentorshipApplication.Create(Guid.NewGuid(), Guid.NewGuid(), new string('A', 60));

        application.UpdateStatus(ApplicationStatus.Pending);

        application.Status.Should().Be(ApplicationStatus.Pending);
        application.IsReadByEmployer.Should().BeFalse();
    }

    [Fact]
    public void MentorshipApplication_MarkRead_SetsIsReadByEmployer()
    {
        var application = MentorshipApplication.Create(Guid.NewGuid(), Guid.NewGuid(), new string('A', 60));

        application.MarkRead();

        application.IsReadByEmployer.Should().BeTrue();
    }

    #endregion
}
