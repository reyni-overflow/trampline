using Trampline.Core.Models.Employee;

namespace Trampline.Contracts.DTOs.Requests;

public record CreateMentorshipRequest
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Address { get; init; }

    public decimal? SalaryFrom { get; init; }

    public decimal? SalaryTo { get; init; }

    public TagRequest[] Tags { get; init; } = [];

    public WorkFormat Format { get; init; } = WorkFormat.Hybrid;

    public int? MaxParticipants { get; init; }

    public string? Duration { get; init; }

    public DateTime? StartDate { get; init; }

    public DateTime? EndedAt { get; init; }
}
