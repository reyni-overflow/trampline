namespace Trampline.Contracts.DTOs.Responses;

public record ContactResponse
{
    public Guid Id { get; set; }

    public Guid ContactUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Patronymic { get; set; } = string.Empty;

    public string? Photo { get; set; }

    public List<string> Skills { get; set; } = [];

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
