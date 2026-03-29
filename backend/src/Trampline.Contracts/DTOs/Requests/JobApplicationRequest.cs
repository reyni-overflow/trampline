namespace Trampline.Contracts.DTOs.Requests;

public record JobApplicationRequest
{
    public Guid JobId { get; set; }

    public string CoverLetter { get; set; } = string.Empty;
}