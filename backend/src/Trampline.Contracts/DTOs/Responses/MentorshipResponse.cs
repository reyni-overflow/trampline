using Trampline.Core.Models.Employee;

namespace Trampline.Contracts.DTOs.Responses;

public record MentorshipResponse
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public string Title { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public double GeoLon { get; set; }

    public double GeoLat { get; set; }

    public string Country { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime EndedAt { get; set; }

    public DateTime? StartDate { get; set; }

    public int? MaxParticipants { get; set; }

    public string? Duration { get; set; }

    public WorkFormat Format { get; set; } = WorkFormat.Hybrid;

    public decimal? SalaryFrom { get; set; }

    public decimal? SalaryTo { get; set; }

    public TagResponse[] Tags { get; set; } = [];

    public List<string> Photos { get; set; } = [];

    public List<string> Videos { get; set; } = [];

    public bool IsActive { get; set; }

    public bool IsFavorited { get; set; }

    public int Views { get; set; } = 0;

    public string CompanyName { get; set; } = string.Empty;
}
