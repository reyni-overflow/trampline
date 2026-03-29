namespace Trampline.Contracts.DTOs.Requests;

public record CreateReviewRequest
{
    public required string Text { get; init; }

    public required int Rating { get; init; }
}
