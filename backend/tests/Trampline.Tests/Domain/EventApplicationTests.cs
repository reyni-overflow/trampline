using FluentAssertions;
using Trampline.Core.Exceptions;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;

namespace Trampline.Tests.Domain;

public class EventApplicationTests
{
    private static readonly string ValidCoverLetter = new('B', 60);

    [Fact]
    public void Create_WithValidData_CreatesApplication()
    {
        var workerId = Guid.NewGuid();
        var eventId = Guid.NewGuid();

        var application = EventApplication.Create(workerId, eventId, ValidCoverLetter);

        application.Id.Should().NotBeEmpty();
        application.WorkerProfileId.Should().Be(workerId);
        application.EventId.Should().Be(eventId);
        application.CoverLetter.Should().Be(ValidCoverLetter);
        application.Status.Should().Be(ApplicationStatus.Pending);
        application.IsReadByEmployer.Should().BeFalse();
    }

    [Fact]
    public void Create_WithShortCoverLetter_ThrowsDomainException()
    {
        var act = () => EventApplication.Create(Guid.NewGuid(), Guid.NewGuid(), "Short");

        act.Should().Throw<DomainException>().WithMessage("*50 characters*");
    }

    [Fact]
    public void Create_WithExactly50Chars_Succeeds()
    {
        var coverLetter = new string('X', 50);

        var application = EventApplication.Create(Guid.NewGuid(), Guid.NewGuid(), coverLetter);

        application.Should().NotBeNull();
    }

    [Fact]
    public void UpdateStatus_ChangesStatusAndMarksRead()
    {
        var application = EventApplication.Create(Guid.NewGuid(), Guid.NewGuid(), ValidCoverLetter);

        application.UpdateStatus(ApplicationStatus.Invited);

        application.Status.Should().Be(ApplicationStatus.Invited);
        application.IsReadByEmployer.Should().BeTrue();
    }

    [Fact]
    public void UpdateStatus_ToPending_DoesNotMarkRead()
    {
        var application = EventApplication.Create(Guid.NewGuid(), Guid.NewGuid(), ValidCoverLetter);

        application.UpdateStatus(ApplicationStatus.Pending);

        application.IsReadByEmployer.Should().BeFalse();
    }

    [Fact]
    public void MarkRead_SetsReadFlag()
    {
        var application = EventApplication.Create(Guid.NewGuid(), Guid.NewGuid(), ValidCoverLetter);

        application.MarkRead();

        application.IsReadByEmployer.Should().BeTrue();
    }
}
