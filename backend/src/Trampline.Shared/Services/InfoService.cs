using IPinfo;
using IPinfo.Models;
using Microsoft.Extensions.Logging;

namespace Trampline.Shared.Services;

public class InfoService(ILogger<InfoService> logger) : IInfoService
{
    public async Task<string> GetLocation(string ip, CancellationToken cancellationToken)
    {
        try
        {
            string token = Environment.GetEnvironmentVariable("IPINFO_TOKEN") ?? "6d81ceaf0cfdf2";
            IPinfoClient client = new IPinfoClient.Builder()
                .AccessToken(token)
                .Build();

            IPResponse ipResponse = await client.IPApi.GetDetailsAsync(ip);

            logger.LogDebug("IP address: {IP}", ipResponse.IP);
            logger.LogDebug("City: {City}", ipResponse.City);
            logger.LogDebug("Region / State: {Region}", ipResponse.Region);
            logger.LogDebug("Country : {Postal}", ipResponse.Postal);
            logger.LogDebug("Country: {Country}", ipResponse.Country);
            logger.LogDebug("Country Name: {CountryName}", ipResponse.CountryName);
            logger.LogDebug("Geographic Coordinate: {Loc}", ipResponse.Loc);
            logger.LogDebug("Continent: {Continent}", ipResponse.Continent?.Name);
            return ipResponse.City ?? "Unknown";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get location for IP {IP}", ip);
            return "Unknown";
        }
    }
}