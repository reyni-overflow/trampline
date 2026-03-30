namespace Trampline.Core.Models;

public class Review
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string AuthorName { get; private set; } = string.Empty;

    public string AuthorRole { get; private set; } = string.Empty;

    public string Text { get; private set; } = string.Empty;

    public int Rating { get; private set; }

    public bool IsApproved { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Review() { }

    public static Review Create(Guid userId, string authorName, string authorRole, string text, int rating)
    {
        return new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AuthorName = authorName,
            AuthorRole = authorRole,
            Text = text,
            Rating = rating,
            IsApproved = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Approve() => IsApproved = true;

    public void Reject() => IsApproved = false;
}
