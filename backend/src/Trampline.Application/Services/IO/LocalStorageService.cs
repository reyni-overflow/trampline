using Microsoft.Extensions.Logging;
using Trampline.Core.Storage;

namespace Trampline.Application.Services.IO;

public class LocalStorageService(ILogger<LocalStorageService> logger) : IStorageService
{
    private static readonly string FilesRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "files"));

    public async Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken ct = default)
    {
        var diskPath = ResolveDiskPath(path);
        var dir = Path.GetDirectoryName(diskPath);
        if (dir != null && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        await using var fileStream = File.Create(diskPath);
        stream.Position = 0;
        await stream.CopyToAsync(fileStream, ct);

        logger.LogInformation("Local: uploaded {Path} ({Size} bytes)", path, stream.Length);
        return path;
    }

    public Task DeleteAsync(string path, CancellationToken ct = default)
    {
        var diskPath = ResolveDiskPath(path);
        if (File.Exists(diskPath))
        {
            File.Delete(diskPath);
            logger.LogInformation("Local: deleted {Path}", path);
        }

        return Task.CompletedTask;
    }

    public Task<Stream?> GetAsync(string path, CancellationToken ct = default)
    {
        var diskPath = ResolveDiskPath(path);
        if (!File.Exists(diskPath))
            return Task.FromResult<Stream?>(null);

        return Task.FromResult<Stream?>(File.OpenRead(diskPath));
    }

    public Task<bool> ExistsAsync(string path, CancellationToken ct = default)
    {
        var diskPath = ResolveDiskPath(path);
        return Task.FromResult(File.Exists(diskPath));
    }

    private static string ResolveDiskPath(string path)
    {
        var clean = Path.GetFileName(path);
        var subDir = path.Contains("/videos/") ? "videos"
            : path.Contains("/photos/") ? "photos"
            : path.Contains("/resumes/") ? "resumes"
            : "files";
        var full = Path.GetFullPath(Path.Combine(FilesRoot, subDir, clean));

        if (!full.StartsWith(FilesRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Path traversal detected");

        return full;
    }
}
