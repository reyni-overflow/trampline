using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.Employees;

public interface IJobService
{
    Task<Result> UpdateApplicationStatusAsync(Guid employeeUserId, Guid applicationId, ApplicationStatus status, CancellationToken cancellationToken);

    Task<Result<IEnumerable<JobApplication>>> GetApplicationsAsync(Guid userId, Guid jobId, CancellationToken cancellationToken);

    Task<Result<Job>> CreateJob(Guid userId, CreateJobRequest request, CancellationToken cancellationToken);

    Task<Result<Job>> ApplicationJobAsync(Guid userId, JobApplicationRequest request, CancellationToken cancellationToken);

    Task<Result<Job>> GetByIdAsync(Guid id, CancellationToken cancellationToken, Guid? userId = null);

    Task<Result<Job>> UpdateAsync(Guid userId, Guid id, UpdateJobRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, Guid employeeId, CancellationToken cancellationToken);

    Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken);
}