using System.ComponentModel.DataAnnotations;
using Trampline.Core.Models;

namespace Trampline.Contracts.DTOs.Responses;

public record JobApplicationResponse
{
    [Key]
    public Guid Id { get; set; }

    public Guid WorkerProfileId { get; set; }

    public WorkerProfileResponse Profile { get; set; } = null!;

    public Guid JobId { get; set; }

    public string? JobTitle { get; set; }

    public string? CompanyName { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public string? CoverLetter { get; set; }

    public bool IsReadByEmployer { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}