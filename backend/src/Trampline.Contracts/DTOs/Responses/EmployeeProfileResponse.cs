using Trampline.Core.Models.Employee;

namespace Trampline.Contracts.DTOs.Responses;

public record EmployeeProfileResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Activity { get; set; } = string.Empty;

    public string? Link { get; set; }

    public List<string> Socials { get; set; } = new();

    public List<string> Photos { get; set; } = new();

    public List<string> Videos { get; set; } = new();

    public bool IsVerified { get; set; } = false;

    public int VerificationLevel { get; set; } = 0;

    public string? VerifiedName { get; set; }

    public EmployeeInfo Info { get; set; } = new();
}