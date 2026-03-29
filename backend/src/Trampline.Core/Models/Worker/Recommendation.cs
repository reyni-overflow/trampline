using System.ComponentModel.DataAnnotations;

namespace Trampline.Core.Models.Worker;

public class Recommendation
{
    [Key]
    public Guid Id { get; private set; }

    public Guid FromUserId { get; private set; }

    public Guid ToUserId { get; private set; }

    public Guid JobId { get; private set; }

    public string? Message { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Recommendation() { }

    public static Recommendation Create(Guid fromUserId, Guid toUserId, Guid jobId, string? message)
    {
        return new Recommendation
        {
            Id = Guid.NewGuid(),
            FromUserId = fromUserId,
            ToUserId = toUserId,
            JobId = jobId,
            Message = message?.Trim()
        };
    }
}
