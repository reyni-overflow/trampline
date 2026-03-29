namespace Trampline.Contracts.DTOs.Requests;

public record ForgotPasswordRequest
{
    public required string Email { get; set; }
}
