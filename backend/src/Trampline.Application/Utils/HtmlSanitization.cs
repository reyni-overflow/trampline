using Ganss.Xss;

namespace Trampline.Application.Utils;

public static class HtmlSanitization
{
    private static readonly HtmlSanitizer Sanitizer = new();

    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input ?? string.Empty;
        return Sanitizer.Sanitize(input);
    }
}
