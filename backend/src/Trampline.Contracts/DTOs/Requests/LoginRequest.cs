namespace Trampline.Contracts.DTOs.Requests;

public record LoginRequest
{
    public required string Contact { get; set; }

    public required string Password { get; set; }
}