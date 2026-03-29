namespace Trampline.Contracts.DTOs.Requests;

public class RecommendRequest
{
    public Guid ToUserId { get; set; }
    public Guid JobId { get; set; }
    public string? Message { get; set; }
}
