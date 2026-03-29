namespace Trampline.Contracts.DTOs.Responses;

public record TagResponse
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required string Category { get; init; }

    public int Lvl { get; init; }
}