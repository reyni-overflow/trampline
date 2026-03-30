namespace Trampline.Contracts.DTOs.Requests;

public record VerifyEmailRequest
{
    public required string Email { get; set; }
    public required string Code { get; set; }
}
