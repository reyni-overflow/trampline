namespace Trampline.Contracts.DTOs.Requests;

public record UpdateJobRequest
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public bool? IsPublished { get; set; }

    public decimal? SalaryFrom { get; init; }

    public decimal? SalaryTo { get; init; }

    public TagRequest[] Tags { get; init; } = [];

    public DateTime? EndedAt { get; init; }
}