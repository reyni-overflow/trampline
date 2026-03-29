using Trampline.Core.Models;

namespace Trampline.Contracts.DTOs.Responses;

public record EventApplicationResponse
{
    public Guid Id { get; set; }

    public string CoverLetter { get; set; } = string.Empty;

    public Guid EventId { get; set; }

    public string? EventTitle { get; set; }

    public string? CompanyName { get; set; }

    public WorkerProfileResponse Profile { get; set; } = null!;

    public Guid WorkerProfileId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsReadByEmployer { get; set; }

    public ApplicationStatus Status { get; set; }
}
