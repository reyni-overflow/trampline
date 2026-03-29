using FluentAssertions;
using Trampline.Core.Models;

namespace Trampline.Tests.Domain;

public class NotificationTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var userId = Guid.NewGuid();

        var notification = Notification.Create(userId, "new_application", "Новый отклик", "Иван откликнулся на вакансию", "/dashboard/jobs/123/responses");

        notification.Id.Should().NotBeEmpty();
        notification.UserId.Should().Be(userId);
        notification.Type.Should().Be("new_application");
        notification.Title.Should().Be("Новый отклик");
        notification.Message.Should().Be("Иван откликнулся на вакансию");
        notification.Link.Should().Be("/dashboard/jobs/123/responses");
        notification.IsRead.Should().BeFalse();
        notification.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithoutLink_SetsLinkNull()
    {
        var notification = Notification.Create(Guid.NewGuid(), "type", "Title", "Message");

        notification.Link.Should().BeNull();
    }

    [Fact]
    public void MarkAsRead_SetsIsReadTrue()
    {
        var notification = Notification.Create(Guid.NewGuid(), "type", "Title", "Message");

        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
    }

    [Fact]
    public void MarkAsRead_CalledTwice_RemainsTrue()
    {
        var notification = Notification.Create(Guid.NewGuid(), "type", "Title", "Message");

        notification.MarkAsRead();
        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
    }
}
