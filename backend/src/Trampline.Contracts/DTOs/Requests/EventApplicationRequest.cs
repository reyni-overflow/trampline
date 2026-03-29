namespace Trampline.Contracts.DTOs.Requests;

public record EventApplicationRequest
{
    public Guid EventId { get; set; }

    public string CoverLetter { get; set; } = string.Empty;
}
