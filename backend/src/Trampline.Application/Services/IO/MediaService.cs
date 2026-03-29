using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trampline.Core.Storage;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.IO;

public class MediaService(ILogger<MediaService> logger, IStorageService storage) : IMediaService
{
    private static readonly Dictionary<string, byte[][]> MagicBytes = new()
    {
        { ".jpg", [new byte[] { 0xFF, 0xD8, 0xFF }] },
        { ".png", [new byte[] { 0x89, 0x50, 0x4E, 0x47 }] },
        { ".webp", [new byte[] { 0x52, 0x49, 0x46, 0x46 }] },
        { ".pdf", [new byte[] { 0x25, 0x50, 0x44, 0x46 }] },
        { ".mp4", [new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 }, new byte[] { 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70 }, new byte[] { 0x00, 0x00, 0x00, 0x1C, 0x66, 0x74, 0x79, 0x70 }] },
        { ".webm", [new byte[] { 0x1A, 0x45, 0xDF, 0xA3 }] },
        { ".doc", [new byte[] { 0xD0, 0xCF, 0x11, 0xE0 }] },
        { ".docx", [new byte[] { 0x50, 0x4B, 0x03, 0x04 }] }
    };

    private static readonly Dictionary<string, string> MimeToExt = new()
    {
        { "image/jpeg", ".jpg" },
        { "image/png", ".png" },
        { "image/webp", ".webp" },
        { "video/mp4", ".mp4" },
        { "video/webm", ".webm" },
        { "application/pdf", ".pdf" },
        { "application/msword", ".doc" },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx" }
    };

    public async Task<Result<string>> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length <= 0 || file.Length > 100_000_000)
        {
            logger.LogWarning("File upload rejected: size {Size} bytes", file.Length);
            return Result<string>.Failure(new ErrorDetail(nameof(file), "File size must be between 1 byte and 100 MB", 400));
        }

        if (!MimeToExt.TryGetValue(file.ContentType.ToLower(), out var expectedExt))
        {
            logger.LogWarning("File upload rejected: type {ContentType}", file.ContentType);
            return Result<string>.Failure(new ErrorDetail(nameof(file), $"File type '{file.ContentType}' is not allowed", 400));
        }

        using var headerStream = file.OpenReadStream();
        var header = new byte[8];
        var bytesRead = await headerStream.ReadAsync(header, cancellationToken);

        if (bytesRead < 4 || !ValidateMagicBytes(expectedExt, header))
        {
            logger.LogWarning("File upload rejected: magic bytes mismatch for {ContentType}", file.ContentType);
            return Result<string>.Failure(new ErrorDetail(nameof(file), "File content does not match declared type", 400));
        }

        if (ScanForMaliciousContent(headerStream, expectedExt))
        {
            logger.LogWarning("File upload rejected: malicious content detected in {ContentType}", file.ContentType);
            return Result<string>.Failure(new ErrorDetail(nameof(file), "File rejected: suspicious content detected", 400));
        }

        var uniqueName = $"{Guid.NewGuid():N}{expectedExt}";
        var subDir = GetSubDirectory(file.ContentType, expectedExt);
        var storagePath = $"/files/{subDir}/{uniqueName}";

        headerStream.Position = 0;
        await storage.UploadAsync(headerStream, storagePath, file.ContentType, cancellationToken);

        logger.LogInformation("File uploaded: {Path}, size: {Size} bytes", storagePath, file.Length);
        return Result<string>.Success(storagePath);
    }

    public async Task<Result> DeleteFile(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
            return Result.Failure(new ErrorDetail("path", "File path is empty", 400));

        var fileName = Path.GetFileName(filePath);
        if (string.IsNullOrEmpty(fileName) || filePath.Contains(".."))
        {
            logger.LogWarning("Path traversal attempt detected: {Path}", filePath);
            return Result.Failure(new ErrorDetail("path", "Invalid file path", 400));
        }

        try
        {
            await storage.DeleteAsync(filePath, cancellationToken);
            logger.LogInformation("File deleted: {Path}", filePath);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to delete file: {Path}", filePath);
            return Result.Failure(new ErrorDetail("path", "Failed to delete file", 500));
        }
    }

    private static string GetSubDirectory(string contentType, string ext)
    {
        if (contentType.StartsWith("video/"))
            return "videos";
        if (contentType.StartsWith("image/"))
            return "photos";
        if (ext is ".pdf" or ".doc" or ".docx")
            return "resumes";
        return "files";
    }

    private static readonly byte[][] DangerousPatterns =
    [
        "<script"u8.ToArray(),
        "<?php"u8.ToArray(),
        "<%@"u8.ToArray(),
        "<iframe"u8.ToArray(),
        "javascript:"u8.ToArray(),
    ];

    private static bool ScanForMaliciousContent(Stream stream, string ext)
    {
        if (ext is ".mp4" or ".webm")
            return false;

        stream.Position = 0;
        var bufferSize = (int)Math.Min(stream.Length, 8192);
        var buffer = new byte[bufferSize];
        var read = stream.Read(buffer, 0, bufferSize);

        if (read == 0)
            return false;

        foreach (var pattern in DangerousPatterns)
        {
            if (ContainsPattern(buffer.AsSpan(0, read), pattern))
                return true;
        }

        if (ext is ".pdf")
        {
            ReadOnlySpan<byte> span = buffer.AsSpan(0, read);
            if (ContainsPattern(span, "/JavaScript"u8.ToArray()) ||
                ContainsPattern(span, "/JS "u8.ToArray()) ||
                ContainsPattern(span, "/OpenAction"u8.ToArray()) ||
                ContainsPattern(span, "/Launch"u8.ToArray()) ||
                ContainsPattern(span, "/SubmitForm"u8.ToArray()) ||
                ContainsPattern(span, "/ImportData"u8.ToArray()) ||
                ContainsPattern(span, "/RichMedia"u8.ToArray()) ||
                ContainsPattern(span, "/EmbeddedFile"u8.ToArray()) ||
                ContainsPattern(span, "/XFA"u8.ToArray()) ||
                (ContainsPattern(span, "/AA"u8.ToArray()) && ContainsPattern(span, "/JavaScript"u8.ToArray())))
                return true;
        }

        return false;
    }

    private static bool ContainsPattern(ReadOnlySpan<byte> data, byte[] pattern)
    {
        if (pattern.Length > data.Length)
            return false;

        for (int i = 0; i <= data.Length - pattern.Length; i++)
        {
            if (data.Slice(i, pattern.Length).SequenceEqual(pattern))
                return true;
        }

        return false;
    }

    private static bool ValidateMagicBytes(string ext, byte[] header)
    {
        if (!MagicBytes.TryGetValue(ext, out var signatures))
            return false;

        return signatures.Any(sig => sig.Length <= header.Length && sig.SequenceEqual(header[..sig.Length]));
    }
}
