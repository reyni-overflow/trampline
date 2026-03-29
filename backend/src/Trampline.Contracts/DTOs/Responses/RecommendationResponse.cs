namespace Trampline.Contracts.DTOs.Responses;

public record RecommendationResponse
{
    public Guid Id { get; set; }

    public Guid FromUserId { get; set; }

    public string FromUserName { get; set; } = string.Empty;

    public Guid JobId { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string? Message { get; set; }

    public DateTime CreatedAt { get; set; }
}
