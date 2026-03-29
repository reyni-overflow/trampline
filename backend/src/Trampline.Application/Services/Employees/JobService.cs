using System.Globalization;
using Microsoft.Extensions.Logging;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Application.Utils;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.Employees;

public class JobService(
    ILogger<JobService> logger,
    IJobRepository repository,
    IEmployeeRepository employeeRepository,
    IUserService userService,
    IJobApplicationRepository jobApplicationRepository,
    IDaDataService daDataService) : IJobService
{
    public async Task<Result> UpdateApplicationStatusAsync(Guid employeeUserId, Guid applicationId, ApplicationStatus status, CancellationToken cancellationToken)
    {
        var application = await jobApplicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
            return Result.Failure(new ErrorDetail("application", "Application not found", 404));

        var job = await repository.GetByIdAsync(application.JobId, cancellationToken);
        if (job == null)
            return Result.Failure(new ErrorDetail("job", "Job not found", 404));

        if (job.UserId != employeeUserId)
            return Result.Failure(new ErrorDetail("user", "Access denied", 403));

        application.UpdateStatus(status);
        await jobApplicationRepository.UpdateAsync(application, cancellationToken);

        logger.LogInformation("Application {AppId} status changed to {Status}", applicationId, status);
        return Result.Success();
    }

    public async Task<Result<IEnumerable<JobApplication>>> GetApplicationsAsync(Guid userId, Guid jobId, CancellationToken cancellationToken)
    {
        var findJob = await repository.GetByIdAsync(jobId, cancellationToken);
        var findEmployee = await employeeRepository.GetByUserIdAsync(userId, cancellationToken);

        if (findJob == null)
        {
            return Result<IEnumerable<JobApplication>>.Failure(new ErrorDetail(nameof(jobId), "job not found", 404));
        }

        if (findEmployee == null || findJob.UserId != findEmployee.UserId)
        {
            return Result<IEnumerable<JobApplication>>.Failure(new ErrorDetail(nameof(userId), "Access denied", 403));
        }

        return Result<IEnumerable<JobApplication>>.Success(findJob.JobApplications);
    }

    public async Task<Result<Job>> CreateJob(Guid userId, CreateJobRequest request, CancellationToken cancellationToken)
    {
        var findEmployee = await userService.GetByIdAsync(userId, cancellationToken);

        if (findEmployee == null)
        {
            return Result<Job>.Failure(new ErrorDetail("employee", "employee not found", 404));
        }
        else if (findEmployee.EmployeeProfile == null)
        {
            return Result<Job>.Failure(new ErrorDetail("employee", "employee profile doesnt create", 400));
        }
        else if (!findEmployee.EmployeeProfile.IsVerified)
        {
            return Result<Job>.Failure(new ErrorDetail("employee", "Company must be verified to create opportunities", 403));
        }
        else if (findEmployee.IsPrivate)
        {
            return Result<Job>.Failure(new ErrorDetail("employee", "Blocked users cannot create opportunities", 403));
        }

        var title = HtmlSanitization.Sanitize(request.Title);
        var description = HtmlSanitization.Sanitize(request.Description);

        var newJob = Job.Create(findEmployee.EmployeeProfile.Id, userId, title, description,
            request.Type, request.Format);

        var resultGeo = await daDataService.GetGeoByAddress(request.Address, cancellationToken);

        if (resultGeo.IsSuccess
            && double.TryParse(resultGeo.Value!.GeoLat, CultureInfo.InvariantCulture, out var geoLat) && geoLat != 0
            && double.TryParse(resultGeo.Value!.GeoLon, CultureInfo.InvariantCulture, out var geoLon) && geoLon != 0)
        {
            newJob.UpdateGeo(resultGeo.Value!.Address, resultGeo.Value!.City, resultGeo.Value!.Country, resultGeo.Value!.Street,
                geoLat, geoLon);
        }
        else
        {
            var addr = resultGeo.IsSuccess ? resultGeo.Value!.Address : request.Address;
            var city = resultGeo.IsSuccess ? resultGeo.Value!.City : string.Empty;
            var country = resultGeo.IsSuccess ? resultGeo.Value!.Country : string.Empty;
            newJob.UpdateGeo(addr, city, country, string.Empty, 55.7558, 37.6173);
        }

        newJob.UpdateSalary(request.SalaryFrom, request.SalaryTo);
        newJob.UpdateEndedAt(request.EndedAt);

        if (request.Tags?.Length > 0)
        {
            var tags = await repository.GetOrCreateTagsAsync(request.Tags.Select(x => new Tag()
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                Category = x.Category,
                Lvl = x.Lvl,
            }), cancellationToken);
            newJob.UpdateTags(tags.ToArray());
        }

        if (!findEmployee.EmployeeProfile.IsTrusted)
            newJob.SetActive(false);

        findEmployee.EmployeeProfile.Jobs.Add(newJob);
        await repository.AddAsync(newJob, cancellationToken);
        await userService.UpdateAsync(findEmployee, cancellationToken);
        logger.LogInformation("Job created {JobId} by {UserId}", newJob.Id, userId);
        return Result<Job>.Success(newJob);
    }

    public async Task<Result<Job>> ApplicationJobAsync(Guid userId, JobApplicationRequest request, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(userId, cancellationToken);

        if (findUser == null)
        {
            return Result<Job>.Failure(new ErrorDetail(nameof(userId), "user not found", 404));
        }

        if (findUser.WorkerProfile == null)
        {
            return Result<Job>.Failure(new ErrorDetail("profile", "Worker profile not found. Please create your profile first.", 400));
        }

        var findJob = await repository.GetByIdAsync(request.JobId, cancellationToken);

        if (findJob == null)
        {
            return Result<Job>.Failure(new ErrorDetail(nameof(userId), "job not found", 404));
        }

        var existingApplication = findJob.JobApplications
            .Any(a => a.WorkerProfileId == findUser.WorkerProfile.Id);
        if (existingApplication)
            return Result<Job>.Failure(new ErrorDetail("application", "You have already applied to this job", 409));

        var application = JobApplication.Create(findUser.WorkerProfile.Id, request.JobId, request.CoverLetter);

        findJob.JobApplications.Add(application);
        await jobApplicationRepository.AddAsync(application, cancellationToken);
        await repository.UpdateAsync(findJob, cancellationToken);

        logger.LogInformation("User {UserId} applied to job {JobId}", userId, request.JobId);
        return Result<Job>.Success(findJob);
    }

    public async Task<Result<Job>> GetByIdAsync(Guid id, CancellationToken cancellationToken, Guid? userId = null)
    {
        logger.LogDebug("GetById job {JobId}", id);
        var findJob = await repository.GetByIdAsync(id, cancellationToken);
        if (findJob == null)
        {
            return Result<Job>.Failure(new ErrorDetail(nameof(id), "job not found", 404));
        }

        if (userId != null && !findJob.UserViews.Contains(userId.Value))
        {
            findJob.AddViews(userId.Value);
            await repository.UpdateAsync(findJob, cancellationToken);
        }

        return Result<Job>.Success(findJob);
    }

    public async Task<Result<Job>> UpdateAsync(Guid userId, Guid id, UpdateJobRequest request, CancellationToken cancellationToken)
    {
        var findJob = await repository.GetByIdAsync(id, cancellationToken);
        var findEmployee = await employeeRepository.GetByUserIdAsync(userId, cancellationToken);

        if (findJob == null)
        {
            return Result<Job>.Failure(new ErrorDetail(nameof(id), "job not found", 404));
        }

        if (findEmployee == null || findJob.EmployeeId != findEmployee.Id)
        {
            return Result<Job>.Failure(new ErrorDetail(nameof(userId), "Access denied", 403));
        }

        findJob.Update(request.Title, request.Description, request.Address, request.City, request.Country,
            request.IsActive);

        findJob.UpdateSalary(request.SalaryFrom, request.SalaryTo);
        findJob.UpdateEndedAt(request.EndedAt);

        if (request.Tags.Length > 0)
        {
            var tags = await repository.GetOrCreateTagsAsync(request.Tags.Select(x => new Tag()
            {
                Id = Guid.NewGuid(),
                Name = x.Name,
                Category = x.Category,
                Lvl = x.Lvl,
            }), cancellationToken);
            findJob.UpdateTags(tags.ToArray());
        }

        await repository.UpdateAsync(findJob, cancellationToken);
        logger.LogInformation("Job updated {JobId}", id);
        return Result<Job>.Success(findJob);
    }

    public async Task DeleteAsync(Guid id, Guid employeeId, CancellationToken cancellationToken)
    {
        var find = await repository.GetByEmployeeAsync(id, employeeId, cancellationToken);

        if (find != null)
        {
            find.SoftDelete();
            await repository.UpdateAsync(find, cancellationToken);
            logger.LogInformation("Job soft-deleted {JobId}", id);
        }
    }

    public async Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await repository.GetAllAsync(cancellationToken);
    }
}