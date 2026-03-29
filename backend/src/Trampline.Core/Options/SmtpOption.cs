namespace Trampline.Core.Options;

public class SmtpOption
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "noreply@trampline.org";
    public string FromName { get; set; } = "Трамплин";
    public bool UseSsl { get; set; } = true;
}
