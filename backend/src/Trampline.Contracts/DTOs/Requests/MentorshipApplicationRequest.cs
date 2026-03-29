namespace Trampline.Contracts.DTOs.Requests;

public record MentorshipApplicationRequest
{
    public Guid MentorshipId { get; set; }

    public string CoverLetter { get; set; } = string.Empty;
}
