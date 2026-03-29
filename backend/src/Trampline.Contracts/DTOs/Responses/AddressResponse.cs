namespace Trampline.Contracts.DTOs.Responses;

public record AddressResponse
{
    public string Address { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string GeoLon { get; set; } = string.Empty;

    public string GeoLat { get; set; } = string.Empty;
}