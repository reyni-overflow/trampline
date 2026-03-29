using FluentAssertions;
using Trampline.Core.Exceptions;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;

namespace Trampline.Tests.Domain;

public class JobApplicationTests
{
    private static readonly string ValidCoverLetter = new('A', 60);

    [Fact]
    public void Create_WithValidData_CreatesApplication()
    {
        var workerId = Guid.NewGuid();
        var jobId = Guid.NewGuid();

        var application = JobApplication.Create(workerId, jobId, ValidCoverLetter);

        application.Id.Should().NotBeEmpty();
        application.WorkerProfileId.Should().Be(workerId);
        application.JobId.Should().Be(jobId);
        application.CoverLetter.Should().Be(ValidCoverLetter);
        application.Status.Should().Be(ApplicationStatus.Pending);
        application.IsReadByEmployer.Should().BeFalse();
        application.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Short")]
    [InlineData("   ")]
    public void Create_WithShortCoverLetter_ThrowsDomainException(string coverLetter)
    {
        var act = () => JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), coverLetter);

        act.Should().Throw<DomainException>().WithMessage("*50 characters*");
    }

    [Fact]
    public void Create_WithExactly49Chars_ThrowsDomainException()
    {
        var coverLetter = new string('A', 49);

        var act = () => JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), coverLetter);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithExactly50Chars_Succeeds()
    {
        var coverLetter = new string('A', 50);

        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), coverLetter);

        application.Should().NotBeNull();
    }

    [Fact]
    public void Create_TrimsCoverLetter()
    {
        var coverLetter = "  " + new string('A', 55) + "  ";

        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), coverLetter);

        application.CoverLetter.Should().NotStartWith(" ");
        application.CoverLetter.Should().NotEndWith(" ");
    }

    [Fact]
    public void UpdateStatus_ChangesStatus()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), ValidCoverLetter);

        application.UpdateStatus(ApplicationStatus.Viewed);

        application.Status.Should().Be(ApplicationStatus.Viewed);
    }

    [Fact]
    public void UpdateStatus_ToNonPending_MarksAsRead()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), ValidCoverLetter);

        application.UpdateStatus(ApplicationStatus.Invited);

        application.IsReadByEmployer.Should().BeTrue();
    }

    [Fact]
    public void UpdateStatus_ToPending_DoesNotMarkAsRead()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), ValidCoverLetter);

        application.UpdateStatus(ApplicationStatus.Pending);

        application.IsReadByEmployer.Should().BeFalse();
    }

    [Theory]
    [InlineData(ApplicationStatus.Viewed)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Invited)]
    [InlineData(ApplicationStatus.Hired)]
    [InlineData(ApplicationStatus.Withdrawn)]
    public void UpdateStatus_AllNonPendingStatuses_MarkAsRead(ApplicationStatus status)
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), ValidCoverLetter);

        application.UpdateStatus(status);

        application.IsReadByEmployer.Should().BeTrue();
    }

    [Fact]
    public void MarkRead_SetsIsReadByEmployerTrue()
    {
        var application = JobApplication.Create(Guid.NewGuid(), Guid.NewGuid(), ValidCoverLetter);

        application.MarkRead();

        application.IsReadByEmployer.Should().BeTrue();
    }
}
