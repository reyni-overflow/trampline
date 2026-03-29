using FluentAssertions;
using Trampline.Core.Models.Worker;

namespace Trampline.Tests.Domain;

public class ContactTests
{
    [Fact]
    public void Create_SetsCorrectProperties()
    {
        var requesterId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();

        var contact = Contact.Create(requesterId, receiverId);

        contact.Id.Should().NotBeEmpty();
        contact.RequesterId.Should().Be(requesterId);
        contact.ReceiverId.Should().Be(receiverId);
        contact.Status.Should().Be(ContactStatus.Pending);
        contact.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Accept_ChangesStatusToAccepted()
    {
        var contact = Contact.Create(Guid.NewGuid(), Guid.NewGuid());

        contact.Accept();

        contact.Status.Should().Be(ContactStatus.Accepted);
    }

    [Fact]
    public void Decline_ChangesStatusToDeclined()
    {
        var contact = Contact.Create(Guid.NewGuid(), Guid.NewGuid());

        contact.Decline();

        contact.Status.Should().Be(ContactStatus.Declined);
    }

    [Fact]
    public void StatusTransitions_FromPendingToAccepted()
    {
        var contact = Contact.Create(Guid.NewGuid(), Guid.NewGuid());
        contact.Status.Should().Be(ContactStatus.Pending);

        contact.Accept();
        contact.Status.Should().Be(ContactStatus.Accepted);
    }

    [Fact]
    public void StatusTransitions_FromPendingToDeclined()
    {
        var contact = Contact.Create(Guid.NewGuid(), Guid.NewGuid());
        contact.Status.Should().Be(ContactStatus.Pending);

        contact.Decline();
        contact.Status.Should().Be(ContactStatus.Declined);
    }
}
