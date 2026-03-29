using Trampline.Core.Models;

namespace Trampline.Contracts.DTOs.Requests;

public record RegisterRequest
{
    public required string Name { get; set; }

    public required string Password { get; set; }

    public required string Email { get; set; }

    public Role Role { get; set; } = Role.Worker;
}