namespace Trampline.Shared.Services;

public interface IInfoService
{
    Task<string> GetLocation(string ip, CancellationToken cancellationToken);
}