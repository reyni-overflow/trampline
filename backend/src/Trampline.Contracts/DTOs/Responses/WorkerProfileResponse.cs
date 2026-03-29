using Trampline.Core.Models.Worker;

namespace Trampline.Contracts.DTOs.Responses;

public record WorkerProfileResponse
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Patronymic { get; set; } = string.Empty;

    public WorkerInfo? Info { get; set; }

    public string? About { get; set; }

    public string? Photo { get; set; }

    public string? Resume { get; set; }

    public List<string> Skills { get; set; } = [];

    public List<string> Repos { get; set; } = [];
}