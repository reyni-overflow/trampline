using System.Globalization;
using Microsoft.Extensions.Logging;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Application.Utils;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.Mentorships;

public class MentorshipService(
    ILogger<MentorshipService> logger,
    IMentorshipRepository repository,
    IEmployeeRepository employeeRepository,
    IUserService userService,
    IMentorshipApplicationRepository mentorshipApplicationRepository,
    IDaDataService daDataService) : IMentorshipService
{
    public async Task<Result> UpdateApplicationStatusAsync(Guid employeeUserId, Guid applicationId, ApplicationStatus status, CancellationToken cancellationToken)
    {
        var application = await mentorshipApplicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
            return Result.Failure(new ErrorDetail("application", "Application not found", 404));

        var mentorship = await repository.GetByIdAsync(application.MentorshipId, cancellationToken);
        if (mentorship == null)
            return Result.Failure(new ErrorDetail("mentorship", "Mentorship not found", 404));

        if (mentorship.UserId != employeeUserId)
            return Result.Failure(new ErrorDetail("user", "Access denied", 403));

        application.UpdateStatus(status);
        await mentorshipApplicationRepository.UpdateAsync(application, cancellationToken);

        logger.LogInformation("Mentorship application {AppId} status changed to {Status}", applicationId, status);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<MentorshipApplication>>> GetApplicationsAsync(Guid userId, Guid mentorshipId, CancellationToken cancellationToken)
    {
        var findMentorship = await repository.GetByIdAsync(mentorshipId, cancellationToken);
        var findEmployee = await employeeRepository.GetByUserIdAsync(userId, cancellationToken);

        if (findMentorship == null)
        {
            return Result<IEnumerable<MentorshipApplication>>.Failure(new ErrorDetail(nameof(mentorshipId), "mentorship not found", 404));
        }

        if (findEmployee == null || findMentorship.UserId != findEmployee.UserId)
        {
            return Result<IEnumerable<MentorshipApplication>>.Failure(new ErrorDetail(nameof(userId), "Access denied", 403));
        }

        return Result<IEnumerable<MentorshipApplication>>.Success(findMentorship.MentorshipApplications);
    }

    public async Task<Result<Mentorship>> CreateMentorshipAsync(Guid userId, CreateMentorshipRequest request, CancellationToken cancellationToken)
    {
        var findEmployee = await userService.GetByIdAsync(userId, cancellationToken);

        if (findEmployee == null)
        {
            return Result<Mentorship>.Failure(new ErrorDetail("employee", "employee not found", 404));
        }
        else if (findEmployee.EmployeeProfile == null)
        {
            return Result<Mentorship>.Failure(new ErrorDetail("employee", "employee profile doesnt create", 400));
        }
        else if (!findEmployee.EmployeeProfile.IsVerified)
        {
            return Result<Mentorship>.Failure(new ErrorDetail("employee", "Company must be verified to create opportunities", 403));
        }
        else if (findEmployee.IsPrivate)
        {
            return Result<Mentorship>.Failure(new ErrorDetail("employee", "Blocked users cannot create opportunities", 403));
        }

        var title = HtmlSanitization.Sanitize(request.Title);
        var description = HtmlSanitization.Sanitize(request.Description);

        var newMentorship = Mentorship.Create(findEmployee.EmployeeProfile.Id, userId, title, description,
            request.Format);

        var resultGeo = await daDataService.GetGeoByAddress(request.Address, cancellationToken);

        if (resultGeo.IsSuccess
            && double.TryParse(resultGeo.Value!.GeoLat, CultureInfo.InvariantCulture, out var geoLat) && geoLat != 0
            && double.TryParse(resultGeo.Value!.GeoLon, CultureInfo.InvariantCulture, out var geoLon) && geoLon != 0)
        {
            newMentorship.UpdateGeo(resultGeo.Value!.Address, resultGeo.Value!.City, resultGeo.Value!.Country, resultGeo.Value!.Street,
                geoLat, geoLon);
        }
        else
        {
            var addr = resultGeo.IsSuccess ? resultGeo.Value!.Address : request.Address;
            var city = resultGeo.IsSuccess ? resultGeo.Value!.City : string.Empty;
            var country = resultGeo.IsSuccess ? resultGeo.Value!.Country : string.Empty;
            newMentorship.UpdateGeo(addr, city, country, string.Empty, 55.7558, 37.6173);
        }

        newMentorship.UpdateSalary(request.SalaryFrom, request.SalaryTo);
        newMentorship.UpdateStartDate(request.StartDate);
        newMentorship.UpdateMaxParticipants(request.MaxParticipants);
        newMentorship.UpdateDuration(request.Duration);
        newMentorship.UpdateEndedAt(request.EndedAt);

        if (request.Tags?.Length > 0)
        {
            var tags = await repository.GetOrCreateTagsAsync(request.Tags.Select(x => new Tag()
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                Category = x.Category,
                Lvl = x.Lvl,
            }), cancellationToken);
            newMentorship.UpdateTags(tags.ToArray());
        }

        if (!findEmployee.EmployeeProfile.IsTrusted)
            newMentorship.SetActive(false);

        findEmployee.EmployeeProfile.Mentorships.Add(newMentorship);
        await repository.AddAsync(newMentorship, cancellationToken);
        await userService.UpdateAsync(findEmployee, cancellationToken);
        logger.LogInformation("Mentorship created {MentorshipId} by {UserId}", newMentorship.Id, userId);
        return Result<Mentorship>.Success(newMentorship);
    }

    public async Task<Result<Mentorship>> ApplicationMentorshipAsync(Guid userId, MentorshipApplicationRequest request, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(userId, cancellationToken);

        if (findUser == null)
        {
            return Result<Mentorship>.Failure(new ErrorDetail(nameof(userId), "user not found", 404));
        }

        if (findUser.WorkerProfile == null)
        {
            return Result<Mentorship>.Failure(new ErrorDetail("profile", "Worker profile not found. Please create your profile first.", 400));
        }

        var findMentorship = await repository.GetByIdAsync(request.MentorshipId, cancellationToken);

        if (findMentorship == null)
        {
            return Result<Mentorship>.Failure(new ErrorDetail(nameof(userId), "mentorship not found", 404));
        }

        var existingApplication = findMentorship.MentorshipApplications
            .Any(a => a.WorkerProfileId == findUser.WorkerProfile.Id);
        if (existingApplication)
            return Result<Mentorship>.Failure(new ErrorDetail("application", "You have already applied to this mentorship", 409));

        var application = MentorshipApplication.Create(findUser.WorkerProfile.Id, request.MentorshipId, request.CoverLetter);

        findMentorship.MentorshipApplications.Add(application);
        await mentorshipApplicationRepository.AddAsync(application, cancellationToken);
        await repository.UpdateAsync(findMentorship, cancellationToken);

        logger.LogInformation("User {UserId} applied to mentorship {MentorshipId}", userId, request.MentorshipId);
        return Result<Mentorship>.Success(findMentorship);
    }

    public async Task<Result<Mentorship>> GetByIdAsync(Guid id, CancellationToken cancellationToken, Guid? userId = null)
    {
        var findMentorship = await repository.GetByIdAsync(id, cancellationToken);
        if (findMentorship == null)
        {
            return Result<Mentorship>.Failure(new ErrorDetail(nameof(id), "mentorship not found", 404));
        }

        if (userId != null && !findMentorship.UserViews.Contains(userId.Value))
        {
            findMentorship.AddViews(userId.Value);
            await repository.UpdateAsync(findMentorship, cancellationToken);
        }

        return Result<Mentorship>.Success(findMentorship);
    }

    public async Task<Result<Mentorship>> UpdateAsync(Guid userId, Guid id, UpdateMentorshipRequest request, CancellationToken cancellationToken)
    {
        var findMentorship = await repository.GetByIdAsync(id, cancellationToken);
        var findEmployee = await employeeRepository.GetByUserIdAsync(userId, cancellationToken);

        if (findMentorship == null)
        {
            return Result<Mentorship>.Failure(new ErrorDetail(nameof(id), "mentorship not found", 404));
        }

        if (findEmployee == null || findMentorship.EmployeeId != findEmployee.Id)
        {
            return Result<Mentorship>.Failure(new ErrorDetail(nameof(userId), "Access denied", 403));
        }

        findMentorship.Update(
            request.Title ?? findMentorship.Title,
            request.Description ?? findMentorship.Description,
            request.Address ?? findMentorship.Address,
            request.City ?? findMentorship.City,
            request.Country ?? findMentorship.Country,
            request.IsActive);
        if (request.SalaryFrom.HasValue || request.SalaryTo.HasValue)
            findMentorship.UpdateSalary(request.SalaryFrom, request.SalaryTo);
        if (request.StartDate.HasValue)
            findMentorship.UpdateStartDate(request.StartDate);
        if (request.MaxParticipants.HasValue)
            findMentorship.UpdateMaxParticipants(request.MaxParticipants);
        if (request.Duration != null)
            findMentorship.UpdateDuration(request.Duration);
        if (request.EndedAt.HasValue)
            findMentorship.UpdateEndedAt(request.EndedAt);

        if (request.Tags.Length > 0)
        {
            var tags = await repository.GetOrCreateTagsAsync(request.Tags.Select(x => new Tag()
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                Category = x.Category,
                Lvl = x.Lvl,
            }), cancellationToken);
            findMentorship.UpdateTags(tags.ToArray());
        }

        await repository.UpdateAsync(findMentorship, cancellationToken);
        logger.LogInformation("Mentorship updated {MentorshipId}", id);
        return Result<Mentorship>.Success(findMentorship);
    }

    public async Task DeleteAsync(Guid id, Guid employeeId, CancellationToken cancellationToken)
    {
        var find = await repository.GetByEmployeeAsync(id, employeeId, cancellationToken);

        if (find != null)
        {
            find.SoftDelete();
            await repository.UpdateAsync(find, cancellationToken);
            logger.LogInformation("Mentorship soft-deleted {MentorshipId}", id);
        }
    }

    public async Task<IEnumerable<Mentorship>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }
}
