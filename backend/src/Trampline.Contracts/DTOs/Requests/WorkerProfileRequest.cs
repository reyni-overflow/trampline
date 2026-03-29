using Trampline.Core.Models.Worker;

namespace Trampline.Contracts.DTOs.Requests;

public record WorkerProfileRequest
{
    public required string Name { get; set; }

    public required string LastName { get; set; }

    public required string Patronymic { get; set; }

    public string? About { get; set; }

    public string? Photo { get; set; }

    public WorkerInfo? Info { get; set; }

    public List<string>? Skills { get; set; }

    public List<string>? Repos { get; set; }
}