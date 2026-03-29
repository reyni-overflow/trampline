namespace Trampline.Contracts.DTOs.Responses;

public record TagStatsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int JobCount { get; set; }

    public int EventCount { get; set; }

    public int TotalCount { get; set; }
}
