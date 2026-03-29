using Microsoft.AspNetCore.Http;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.IO;

public interface IMediaService
{
    Task<Result<string>> UploadFile(IFormFile file, CancellationToken cancellationToken);
    Task<Result> DeleteFile(string filePath, CancellationToken cancellationToken = default);
}
