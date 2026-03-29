using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trampline.Core.Options;

namespace Trampline.Application.Services;

public class EmailService(ILogger<EmailService> logger, IOptions<SmtpOption> options) : IEmailService
{
    private readonly SmtpOption smtp = options.Value;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(smtp.Host);

    public async Task SendPasswordResetCodeAsync(string toEmail, string code, CancellationToken ct = default)
    {
        var subject = "Сброс пароля — Трамплин";
        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Сброс пароля</h2>
            <p style="color: #333; font-size: 15px; line-height: 1.5;">Ваш код для сброса пароля:</p>
            <div style="font-size: 32px; font-weight: bold; letter-spacing: 8px; text-align: center; padding: 20px; background: #f0f4ff; border: 1px solid #d0d9f0; border-radius: 8px; margin: 20px 0; color: #1a1a2e;">
                {code}
            </div>
            <p style="color: #666; font-size: 13px; line-height: 1.5;">Код действителен в течение 15 минут. Если вы не запрашивали сброс пароля, проигнорируйте это письмо.</p>
            """);

        await SendAsync(toEmail, subject, body, ct);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, CancellationToken ct = default)
    {
        var subject = "Добро пожаловать на Трамплин!";
        var body = WrapInLayout($"""
            <h2 style="color: #1a1a2e; margin: 0 0 16px;">Добро пожаловать, {userName}!</h2>
            <p style="color: #333; font-size: 15px; line-height: 1.5;">Вы успешно зарегистрировались на платформе <strong>Трамплин</strong>.</p>
            <p style="color: #333; font-size: 15px; line-height: 1.5;">Теперь вы можете войти в свой аккаунт и начать пользоваться всеми возможностями платформы.</p>
            """);

        await SendAsync(toEmail, subject, body, ct);
    }

    private static string WrapInLayout(string content) => $"""
        <!DOCTYPE html>
        <html lang="ru">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>Трамплин</title>
        </head>
        <body style="margin: 0; padding: 0; background-color: #f4f6fb; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;">
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background-color: #f4f6fb; padding: 32px 16px;">
                <tr>
                    <td align="center">
                        <table role="presentation" width="480" cellpadding="0" cellspacing="0" style="max-width: 480px; width: 100%;">
                            <tr>
                                <td style="text-align: center; padding: 24px 0 20px;">
                                    <span style="font-size: 22px; font-weight: 700; color: #1a1a2e; letter-spacing: 0.5px;">Трамплин</span>
                                </td>
                            </tr>
                            <tr>
                                <td style="background: #ffffff; border-radius: 12px; padding: 32px; box-shadow: 0 1px 4px rgba(0,0,0,0.06);">
                                    {content}
                                </td>
                            </tr>
                            <tr>
                                <td style="text-align: center; padding: 20px 0 0; color: #999; font-size: 12px; line-height: 1.5;">
                                    Это письмо отправлено автоматически, отвечать на него не нужно.
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;

    private async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct)
    {
        if (!IsConfigured)
        {
            logger.LogInformation("SMTP not configured. Email to {To}, subject: {Subject}\n{Body}",
                toEmail, subject, htmlBody);
            return;
        }

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(smtp.FromEmail, smtp.FromName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = htmlBody;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(smtp.Host, smtp.Port);
            client.Credentials = new NetworkCredential(smtp.Username, smtp.Password);
            client.EnableSsl = smtp.UseSsl;

            await client.SendMailAsync(message, ct);
            logger.LogInformation("Email sent to {To}, subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}, subject: {Subject}", toEmail, subject);
        }
    }
}
