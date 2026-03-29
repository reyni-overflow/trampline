using System.Globalization;
using Microsoft.Extensions.Logging;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Application.Utils;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.Events;

public class EventService(
    ILogger<EventService> logger,
    IEventRepository repository,
    IEmployeeRepository employeeRepository,
    IUserService userService,
    IEventApplicationRepository eventApplicationRepository,
    IDaDataService daDataService) : IEventService
{
    public async Task<Result> UpdateApplicationStatusAsync(Guid employeeUserId, Guid applicationId, ApplicationStatus status, CancellationToken cancellationToken)
    {
        var application = await eventApplicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
            return Result.Failure(new ErrorDetail("application", "Application not found", 404));

        var evt = await repository.GetByIdAsync(application.EventId, cancellationToken);
        if (evt == null)
            return Result.Failure(new ErrorDetail("event", "Event not found", 404));

        if (evt.UserId != employeeUserId)
            return Result.Failure(new ErrorDetail("user", "Access denied", 403));

        application.UpdateStatus(status);
        await eventApplicationRepository.UpdateAsync(application, cancellationToken);

        logger.LogInformation("Event application {AppId} status changed to {Status}", applicationId, status);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<EventApplication>>> GetApplicationsAsync(Guid userId, Guid eventId, CancellationToken cancellationToken)
    {
        var findEvent = await repository.GetByIdAsync(eventId, cancellationToken);
        var findEmployee = await employeeRepository.GetByUserIdAsync(userId, cancellationToken);

        if (findEvent == null)
        {
            return Result<IEnumerable<EventApplication>>.Failure(new ErrorDetail(nameof(eventId), "event not found", 404));
        }

        if (findEmployee == null || findEvent.UserId != findEmployee.UserId)
        {
            return Result<IEnumerable<EventApplication>>.Failure(new ErrorDetail(nameof(userId), "Access denied", 403));
        }

        return Result<IEnumerable<EventApplication>>.Success(findEvent.EventApplications);
    }

    public async Task<Result<Event>> CreateEventAsync(Guid userId, CreateEventRequest request, CancellationToken cancellationToken)
    {
        var findEmployee = await userService.GetByIdAsync(userId, cancellationToken);

        if (findEmployee == null)
        {
            return Result<Event>.Failure(new ErrorDetail("employee", "employee not found", 404));
        }
        else if (findEmployee.EmployeeProfile == null)
        {
            return Result<Event>.Failure(new ErrorDetail("employee", "employee profile doesnt create", 400));
        }
        else if (!findEmployee.EmployeeProfile.IsVerified)
        {
            return Result<Event>.Failure(new ErrorDetail("employee", "Company must be verified to create opportunities", 403));
        }
        else if (findEmployee.IsPrivate)
        {
            return Result<Event>.Failure(new ErrorDetail("employee", "Blocked users cannot create opportunities", 403));
        }

        var title = HtmlSanitization.Sanitize(request.Title);
        var description = HtmlSanitization.Sanitize(request.Description);

        var newEvent = Event.Create(findEmployee.EmployeeProfile.Id, userId, title, description,
            request.Format);

        var resultGeo = await daDataService.GetGeoByAddress(request.Address, cancellationToken);

        if (resultGeo.IsSuccess
            && double.TryParse(resultGeo.Value!.GeoLat, CultureInfo.InvariantCulture, out var geoLat) && geoLat != 0
            && double.TryParse(resultGeo.Value!.GeoLon, CultureInfo.InvariantCulture, out var geoLon) && geoLon != 0)
        {
            newEvent.UpdateGeo(resultGeo.Value!.Address, resultGeo.Value!.City, resultGeo.Value!.Country, resultGeo.Value!.Street,
                geoLat, geoLon);
        }
        else
        {
            var addr = resultGeo.IsSuccess ? resultGeo.Value!.Address : request.Address;
            var city = resultGeo.IsSuccess ? resultGeo.Value!.City : string.Empty;
            var country = resultGeo.IsSuccess ? resultGeo.Value!.Country : string.Empty;
            newEvent.UpdateGeo(addr, city, country, string.Empty, 55.7558, 37.6173);
        }

        newEvent.UpdateSalary(request.SalaryFrom, request.SalaryTo);
        newEvent.UpdateStartDate(request.StartDate);
        newEvent.UpdateEndedAt(request.EndedAt);

        if (request.Tags?.Length > 0)
        {
            var tags = await repository.GetOrCreateTagsAsync(request.Tags.Select(x => new Tag()
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                Category = x.Category,
                Lvl = x.Lvl,
            }), cancellationToken);
            newEvent.UpdateTags(tags.ToArray());
        }

        if (!findEmployee.EmployeeProfile.IsTrusted)
            newEvent.SetActive(false);

        findEmployee.EmployeeProfile.Events.Add(newEvent);
        await repository.AddAsync(newEvent, cancellationToken);
        await userService.UpdateAsync(findEmployee, cancellationToken);
        logger.LogInformation("Event created {EventId} by {UserId}", newEvent.Id, userId);
        return Result<Event>.Success(newEvent);
    }

    public async Task<Result<Event>> ApplicationEventAsync(Guid userId, EventApplicationRequest request, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(userId, cancellationToken);

        if (findUser == null)
        {
            return Result<Event>.Failure(new ErrorDetail(nameof(userId), "user not found", 404));
        }

        if (findUser.WorkerProfile == null)
        {
            return Result<Event>.Failure(new ErrorDetail("profile", "Worker profile not found. Please create your profile first.", 400));
        }

        var findEvent = await repository.GetByIdAsync(request.EventId, cancellationToken);

        if (findEvent == null)
        {
            return Result<Event>.Failure(new ErrorDetail(nameof(userId), "event not found", 404));
        }

        var existingApplication = findEvent.EventApplications
            .Any(a => a.WorkerProfileId == findUser.WorkerProfile.Id);
        if (existingApplication)
            return Result<Event>.Failure(new ErrorDetail("application", "You have already applied to this event", 409));

        var application = EventApplication.Create(findUser.WorkerProfile.Id, request.EventId, request.CoverLetter);

        findEvent.EventApplications.Add(application);
        await eventApplicationRepository.AddAsync(application, cancellationToken);
        await repository.UpdateAsync(findEvent, cancellationToken);

        logger.LogInformation("User {UserId} applied to event {EventId}", userId, request.EventId);
        return Result<Event>.Success(findEvent);
    }

    public async Task<Result<Event>> GetByIdAsync(Guid id, CancellationToken cancellationToken, Guid? userId = null)
    {
        var findEvent = await repository.GetByIdAsync(id, cancellationToken);
        if (findEvent == null)
        {
            return Result<Event>.Failure(new ErrorDetail(nameof(id), "event not found", 404));
        }

        if (userId != null && !findEvent.UserViews.Contains(userId.Value))
        {
            findEvent.AddViews(userId.Value);
            await repository.UpdateAsync(findEvent, cancellationToken);
        }

        return Result<Event>.Success(findEvent);
    }

    public async Task<Result<Event>> UpdateAsync(Guid userId, Guid id, UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var findEvent = await repository.GetByIdAsync(id, cancellationToken);
        var findEmployee = await employeeRepository.GetByUserIdAsync(userId, cancellationToken);

        if (findEvent == null)
        {
            return Result<Event>.Failure(new ErrorDetail(nameof(id), "event not found", 404));
        }

        if (findEmployee == null || findEvent.EmployeeId != findEmployee.Id)
        {
            return Result<Event>.Failure(new ErrorDetail(nameof(userId), "Access denied", 403));
        }

        findEvent.Update(request.Title, request.Description, request.Address, request.City, request.Country,
            request.IsActive);
        findEvent.UpdateSalary(request.SalaryFrom, request.SalaryTo);
        findEvent.UpdateStartDate(request.StartDate);
        findEvent.UpdateEndedAt(request.EndedAt);

        if (request.Tags.Length > 0)
        {
            var tags = await repository.GetOrCreateTagsAsync(request.Tags.Select(x => new Tag()
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                Category = x.Category,
                Lvl = x.Lvl,
            }), cancellationToken);
            findEvent.UpdateTags(tags.ToArray());
        }

        await repository.UpdateAsync(findEvent, cancellationToken);
        logger.LogInformation("Event updated {EventId}", id);
        return Result<Event>.Success(findEvent);
    }

    public async Task DeleteAsync(Guid id, Guid employeeId, CancellationToken cancellationToken)
    {
        var find = await repository.GetByEmployeeAsync(id, employeeId, cancellationToken);

        if (find != null)
        {
            find.SoftDelete();
            await repository.UpdateAsync(find, cancellationToken);
            logger.LogInformation("Event soft-deleted {EventId}", id);
        }
    }

    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }
}