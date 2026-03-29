using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models.Worker;

namespace Trampline.Contracts.Extensions;

public static class WorkerExtensions
{
    public static WorkerProfileResponse ToWorkerProfileResponse(this WorkerProfile worker)
    {
        return new WorkerProfileResponse
        {
            Id = worker.Id,
            UserId = worker.UserId,
            LastName = worker.LastName,
            Patronymic = worker.Patronymic,
            About = worker.About,
            Info = worker.Info,
            Photo = worker.Photo,
            Name = worker.Name,
            Resume = worker.Resume,
            Skills = worker.Skills,
            Repos = worker.Repos,
        };
    }
}