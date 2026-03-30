using Dadata;
using Dadata.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Options;
using Trampline.Shared.Results;

namespace Trampline.Application.Services;

public class DaDataService(ILogger<DaDataService> logger, IOptions<DaDataOption> options) : IDaDataService
{
    private readonly SuggestClientAsync client = new(options.Value.Token, options.Value.Secret);
    private readonly CleanClientAsync clientClean = new(options.Value.Token, options.Value.Secret);

    public async Task<Result<FindResponse>> FindParty(string inn,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(inn) || !System.Text.RegularExpressions.Regex.IsMatch(inn, @"^\d{10}(\d{2})?$"))
            return Result<FindResponse>.Failure(new ErrorDetail(nameof(inn), "Invalid INN format. Must be 10 or 12 digits.", 400));

        try
        {
            logger.LogInformation("Querying DaData FindParty for INN {Inn}", inn);
            var query = new FindPartyRequest(inn) { branch_type = PartyBranchType.MAIN };

            var result = await client.FindParty(query, cancellationToken);

            if (result.suggestions.Count > 0)
            {
                var response = new FindResponse()
                {
                    Value = result.suggestions[0].value,
                    Inn = result.suggestions[0].data.inn,
                    ORGN = result.suggestions[0].data.ogrn,
                    Kpp = result.suggestions[0].data.kpp,
                    Type = result.suggestions[0].data.type.ToString()
                };

                logger.LogInformation("DaData FindParty found: {Value}", response.Value);
                return Result<FindResponse>.Success(response);
            }

            return Result<FindResponse>.Failure(new ErrorDetail(nameof(inn), "Unable to find party", 404));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DaData API error");
            return Result<FindResponse>.Failure(new ErrorDetail("dadata", "Address service unavailable", 503));
        }
    }

    public async Task<Result<AddressResponse>> GetGeoByAddress(string address, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Querying DaData GetGeoByAddress for {Address}", address);
            var result = await clientClean.Clean<Address>(address, cancellationToken);

            if (result != null)
            {
                logger.LogInformation("DaData GetGeoByAddress found: {Address}", result.street);
                return Result<AddressResponse>.Success(new AddressResponse()
                {
                    Address = result.result ?? result.street ?? address,
                    City = result.region_with_type ?? result.city ?? string.Empty,
                    Country = result.country ?? string.Empty,
                    GeoLat = result.geo_lat ?? string.Empty,
                    GeoLon = result.geo_lon ?? string.Empty,
                    Street = result.street ?? string.Empty,
                });
            }

            return Result<AddressResponse>.Failure(new ErrorDetail(nameof(address), "Unable to find address", 404));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DaData API error");
            return Result<AddressResponse>.Failure(new ErrorDetail("dadata", "Address service unavailable", 503));
        }
    }
}