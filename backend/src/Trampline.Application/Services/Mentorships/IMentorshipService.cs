using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.Mentorships;

public interface IMentorshipService
{
    Task<Result> UpdateApplicationStatusAsync(Guid employeeUserId, Guid applicationId, ApplicationStatus status, CancellationToken cancellationToken);

    Task<Result<IEnumerable<MentorshipApplication>>> GetApplicationsAsync(Guid userId, Guid mentorshipId, CancellationToken cancellationToken);

    Task<Result<Mentorship>> CreateMentorshipAsync(Guid userId, CreateMentorshipRequest request, CancellationToken cancellationToken);

    Task<Result<Mentorship>> ApplicationMentorshipAsync(Guid userId, MentorshipApplicationRequest request, CancellationToken cancellationToken);

    Task<Result<Mentorship>> GetByIdAsync(Guid id, CancellationToken cancellationToken, Guid? userId = null);

    Task<Result<Mentorship>> UpdateAsync(Guid userId, Guid id, UpdateMentorshipRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, Guid employeeId, CancellationToken cancellationToken);

    Task<IEnumerable<Mentorship>> GetAllAsync(CancellationToken cancellationToken);
}
