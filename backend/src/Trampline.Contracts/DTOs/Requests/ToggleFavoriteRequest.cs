namespace Trampline.Contracts.DTOs.Requests;

public class ToggleFavoriteRequest
{
    public Guid TargetId { get; set; }
    public string Type { get; set; } = "Job";
}
