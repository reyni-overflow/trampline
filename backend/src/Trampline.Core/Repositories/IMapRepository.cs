namespace Trampline.Core.Repositories;

public interface IMapRepository
{
    Task<IEnumerable<object>> GetMarkersAsync(CancellationToken ct);
}
