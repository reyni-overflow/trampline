using System.ComponentModel.DataAnnotations;

namespace Trampline.Core.Models.Worker;

public class Contact
{
    [Key]
    public Guid Id { get; private set; }

    public Guid RequesterId { get; private set; }

    public Guid ReceiverId { get; private set; }

    public ContactStatus Status { get; private set; } = ContactStatus.Pending;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Contact() { }

    public static Contact Create(Guid requesterId, Guid receiverId)
    {
        return new Contact
        {
            Id = Guid.NewGuid(),
            RequesterId = requesterId,
            ReceiverId = receiverId
        };
    }

    public void Accept() => Status = ContactStatus.Accepted;

    public void Decline() => Status = ContactStatus.Declined;
}

public enum ContactStatus
{
    Pending,
    Accepted,
    Declined
}
