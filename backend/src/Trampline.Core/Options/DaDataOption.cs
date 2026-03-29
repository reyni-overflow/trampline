namespace Trampline.Core.Options;

public record DaDataOption
{
    public required string Token { get; set; }

    public required string Secret { get; set; }
}