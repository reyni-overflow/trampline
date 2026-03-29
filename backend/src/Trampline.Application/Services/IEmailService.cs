namespace Trampline.Application.Services;

public interface IEmailService
{
    Task SendPasswordResetCodeAsync(string toEmail, string code, CancellationToken ct = default);
    Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken ct = default);
    bool IsConfigured { get; }
}
