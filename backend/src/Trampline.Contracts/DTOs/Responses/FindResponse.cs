namespace Trampline.Contracts.DTOs.Responses;

public record FindResponse
{
    public string Value { get; set; } = string.Empty;

    public string Inn { get; set; } = string.Empty;

    public string Kpp { get; set; } = string.Empty;

    public string ORGN { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}