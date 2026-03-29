using Trampline.Core.Models;

namespace Trampline.Contracts.DTOs.Responses;

public record MentorshipApplicationResponse
{
    public Guid Id { get; set; }

    public string CoverLetter { get; set; } = string.Empty;

    public Guid MentorshipId { get; set; }

    public string? MentorshipTitle { get; set; }

    public string? CompanyName { get; set; }

    public WorkerProfileResponse Profile { get; set; } = null!;

    public Guid WorkerProfileId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsReadByEmployer { get; set; }

    public ApplicationStatus Status { get; set; }
}
