using Trampline.Core.Models;

namespace Trampline.Contracts.DTOs.Responses;

public record UserResponse
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string Nickname { get; init; }

    public string? Avatar { get; init; }

    public Role Role { get; init; }

    public bool IsPrivate { get; init; }

    public bool HideApplications { get; init; }

    public bool HideResume { get; init; }

    public bool IsTotpEnabled { get; init; }

    public bool IsSuperAdmin { get; init; }

    public bool MustChangePassword { get; init; }

    public WorkerProfileResponse? WorkerProfile { get; init; }

    public EmployeeProfileResponse? EmployeeProfile { get; init; }
}