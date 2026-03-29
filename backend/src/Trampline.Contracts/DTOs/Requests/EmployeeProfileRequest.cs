using Trampline.Core.Models.Employee;

namespace Trampline.Contracts.DTOs.Requests;

public record EmployeeProfileRequest
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Activity { get; init; }

    public string? Link { get; init; }

    public EmployeeInfo Info { get; init; } = new();
}