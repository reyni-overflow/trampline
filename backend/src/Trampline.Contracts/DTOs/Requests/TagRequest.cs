namespace Trampline.Contracts.DTOs.Requests;

public record TagRequest
{
    public required string Name { get; init; }

    public required string Category { get; init; }

    public int Lvl { get; init; }
}