using System.ComponentModel.DataAnnotations;

namespace Trampline.Core.Models;

public class Favorite
{
    [Key]
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid TargetId { get; private set; }

    public FavoriteType Type { get; private set; }

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Favorite() { }

    public static Favorite Create(Guid userId, Guid targetId, FavoriteType type)
    {
        return new Favorite
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetId = targetId,
            Type = type
        };
    }
}

public enum FavoriteType
{
    Job,
    Company,
    Event,
    Mentorship
}
