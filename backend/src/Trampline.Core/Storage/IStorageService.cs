namespace Trampline.Core.Storage;

public interface IStorageService
{
    Task<string> UploadAsync(Stream stream, string path, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string path, CancellationToken ct = default);
    Task<Stream?> GetAsync(string path, CancellationToken ct = default);
    Task<bool> ExistsAsync(string path, CancellationToken ct = default);
}
