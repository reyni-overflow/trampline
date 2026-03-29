using System.ComponentModel.DataAnnotations;
using Trampline.Core.Exceptions;
using Trampline.Core.Models.Worker;

namespace Trampline.Core.Models.Employee;

public class JobApplication
{
    [Key]
    public Guid Id { get; set; }

    public Guid WorkerProfileId { get; set; }

    public WorkerProfile Profile { get; set; } = null!;

    public Guid JobId { get; set; }

    public Job Job { get; set; } = null!;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public string? CoverLetter { get; set; }

    public bool IsReadByEmployer { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static JobApplication Create(Guid workerId, Guid jobId, string coverLetter)
    {
        if (string.IsNullOrWhiteSpace(coverLetter) || coverLetter.Length < 50)
            throw new DomainException("Cover letter must be at least 50 characters");

        return new JobApplication
        {
            Id = Guid.NewGuid(),
            WorkerProfileId = workerId,
            JobId = jobId,
            CoverLetter = coverLetter.Trim(),
        };
    }

    public void UpdateStatus(ApplicationStatus status)
    {
        Status = status;
        if (status != ApplicationStatus.Pending)
            IsReadByEmployer = true;
    }

    public void MarkRead() => IsReadByEmployer = true;
}