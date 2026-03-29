using Trampline.Contracts.DTOs.Responses;
using Trampline.Shared.Results;

namespace Trampline.Application.Services;

public interface IDaDataService
{
    Task<Result<FindResponse>> FindParty(string inn, CancellationToken cancellationToken);

    Task<Result<AddressResponse>> GetGeoByAddress(string address, CancellationToken cancellationToken);
}