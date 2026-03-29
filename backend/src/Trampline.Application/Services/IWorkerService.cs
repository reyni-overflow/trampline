using Microsoft.AspNetCore.Http;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Shared.Results;

namespace Trampline.Application.Services;

public interface IWorkerService
{
    Task<Result<IEnumerable<JobApplication>>> GetApplicationsAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<IEnumerable<EventApplication>>> GetEventApplicationsAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<IEnumerable<MentorshipApplication>>> GetMentorshipApplicationsAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<User>> UpdateProfileAsync(Guid userId, WorkerProfileRequest request,
        CancellationToken cancellationToken);

    Task<Result<User>> UpdateResumeAsync(Guid userId, IFormFile file, CancellationToken cancellationToken);

    Task<Result<User>> UpdateAvatarAsync(Guid userId, IFormFile file, CancellationToken cancellationToken);
}