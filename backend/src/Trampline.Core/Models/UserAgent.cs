namespace Trampline.Core.Models;

public record UserAgent
{
    public string Ip { get; init; }

    public string Agent { get; init; }

    public UserAgent(string ip, string agent)
    {
        Ip = ip;
        Agent = agent ?? string.Empty;
    }
}