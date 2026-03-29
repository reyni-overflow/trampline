namespace Trampline.Contracts.DTOs.Responses;

public record AuthResponse
{
    public Guid Id { get; init; }

    public required string AccessToken { get; init; }

    public required string RefreshToken { get; init; }
}