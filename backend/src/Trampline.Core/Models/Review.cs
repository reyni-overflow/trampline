namespace Trampline.Core.Models;

public class Review
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string AuthorRole { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public int Rating { get; set; }

    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
