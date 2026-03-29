using System.ComponentModel.DataAnnotations;

namespace Trampline.Core.Models;

public class Notification
{
    [Key]
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public string? Link { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Notification() { }

    public static Notification Create(Guid userId, string type, string title, string message, string? link = null)
    {
        return new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link,
            IsRead = false
        };
    }

    public void MarkAsRead() => IsRead = true;
}
