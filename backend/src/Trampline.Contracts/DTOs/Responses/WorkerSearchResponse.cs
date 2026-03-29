namespace Trampline.Contracts.DTOs.Responses;

public record WorkerSearchResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Patronymic { get; set; } = string.Empty;

    public string? Photo { get; set; }

    public string? About { get; set; }

    public List<string> Skills { get; set; } = [];

    public string? University { get; set; }

    public int? Course { get; set; }

    public bool IsPrivate { get; set; }
}
