namespace Trampline.Web.Extensions;

public static class MediaValidationExtensions
{
    private static readonly string[] PhotoExtensions = [".jpg", ".jpeg", ".webp", ".png"];
    private static readonly string[] VideoExtensions = [".mp4", ".webm"];

    public static string? ValidatePhotos(this IFormFile[] files)
    {
        if (files.Length == 0) return "File list is empty.";
        foreach (var file in files)
        {
            if (!PhotoExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                return "Only .jpg, .webp, .png are allowed";
        }
        return null;
    }

    public static string? ValidateVideos(this IFormFile[] files)
    {
        if (files.Length == 0) return "File list is empty.";
        foreach (var file in files)
        {
            if (!VideoExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                return "Only .mp4, .webm are allowed";
        }
        return null;
    }
}
