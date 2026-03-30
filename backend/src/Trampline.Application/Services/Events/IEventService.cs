using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.Events;

public interface IEventService
{
    Task<Result> UpdateApplicationStatusAsync(Guid employeeUserId, Guid applicationId, ApplicationStatus status, CancellationToken cancellationToken);

    Task<Result<IEnumerable<EventApplication>>> GetApplicationsAsync(Guid userId, Guid eventId, CancellationToken cancellationToken);

    Task<Result<Event>> CreateEventAsync(Guid userId, CreateEventRequest request, CancellationToken cancellationToken);

    Task<Result<Event>> ApplicationEventAsync(Guid userId, EventApplicationRequest request, CancellationToken cancellationToken);

    Task<Result<Event>> GetByIdAsync(Guid id, CancellationToken cancellationToken, Guid? userId = null);

    Task<Result<Event>> UpdateAsync(Guid userId, Guid id, UpdateEventRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, Guid employeeId, CancellationToken cancellationToken);

    Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken);
}