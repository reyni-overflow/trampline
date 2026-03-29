using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trampline.Application.Services.IO;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Models.Worker;
using Trampline.Core.Repositories;
using Trampline.Application.Utils;
using Trampline.Shared.Results;

namespace Trampline.Application.Services;

public class WorkerService(
    ILogger<WorkerService> logger,
    IWorkerRepository workerRepository,
    IUserService userService,
    IMediaService mediaService,
    IEventApplicationRepository eventApplicationRepository,
    IMentorshipApplicationRepository mentorshipApplicationRepository) : IWorkerService
{
    public async Task<Result<IEnumerable<JobApplication>>> GetApplicationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(userId, cancellationToken);

        if (result == null)
        {
            return Result<IEnumerable<JobApplication>>.Failure(new ErrorDetail("token", "user not found", 404));
        }

        return Result<IEnumerable<JobApplication>>.Success(result.WorkerProfile!.JobApplications);
    }

    public async Task<Result<IEnumerable<EventApplication>>> GetEventApplicationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(userId, cancellationToken);

        if (result == null)
        {
            return Result<IEnumerable<EventApplication>>.Failure(new ErrorDetail("token", "user not found", 404));
        }

        if (result.WorkerProfile == null)
        {
            return Result<IEnumerable<EventApplication>>.Failure(new ErrorDetail("user", "Worker profile is null", 400));
        }

        var applications = await eventApplicationRepository.GetByWorkerProfileIdAsync(result.WorkerProfile.Id, cancellationToken);
        return Result<IEnumerable<EventApplication>>.Success(applications);
    }

    public async Task<Result<IEnumerable<MentorshipApplication>>> GetMentorshipApplicationsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await userService.GetByIdAsync(userId, cancellationToken);

        if (result == null)
            return Result<IEnumerable<MentorshipApplication>>.Failure(new ErrorDetail("token", "user not found", 404));

        if (result.WorkerProfile == null)
            return Result<IEnumerable<MentorshipApplication>>.Failure(new ErrorDetail("user", "Worker profile is null", 400));

        var applications = await mentorshipApplicationRepository.GetByWorkerProfileIdAsync(result.WorkerProfile.Id, cancellationToken);
        return Result<IEnumerable<MentorshipApplication>>.Success(applications);
    }

    public async Task<Result<User>> UpdateProfileAsync(Guid userId, WorkerProfileRequest request, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id: userId, cancellationToken);

        if (findUser == null)
        {
            return Result<User>.Failure(new ErrorDetail("token", "user not found", 404));
        }

        WorkerInfo? info = null;

        if (request.Info is not null)
        {
            var createdInfo = WorkerInfo.Create(
                request.Info.University,
                request.Info.Course,
                request.Info.AdmissionAt,
                request.Info.GraduationAt);

            if (createdInfo.IsFailure)
            {
                return Result<User>.Failure(createdInfo.Errors.ToArray());
            }

            info = createdInfo.Value!;
        }

        request.About = HtmlSanitization.Sanitize(request.About);

        if (findUser.WorkerProfile == null)
        {
            var profile = WorkerProfile.Create(userId, request.Name, request.LastName,
                request.Patronymic, info, request.About, request.Photo);
            if (request.Skills != null) profile.UpdateSkills(request.Skills);
            if (request.Repos != null) profile.UpdateRepos(request.Repos);
            findUser.SetWorkerProfile(profile);
            await workerRepository.AddAsync(profile, cancellationToken);
            logger.LogInformation("Worker profile created for {UserId}", userId);
        }
        else
        {
            findUser.WorkerProfile.Update(request.Name, request.LastName,
                request.Patronymic, request.About, request.Photo);
            if (info != null) findUser.WorkerProfile.UpdateInfo(info);
            if (request.Skills != null) findUser.WorkerProfile.UpdateSkills(request.Skills);
            if (request.Repos != null) findUser.WorkerProfile.UpdateRepos(request.Repos);
            await workerRepository.UpdateAsync(findUser.WorkerProfile, cancellationToken);
            logger.LogInformation("Worker profile updated for {UserId}", userId);
        }

        await userService.UpdateAsync(findUser, cancellationToken);
        return Result<User>.Success(findUser);
    }

    public async Task<Result<User>> UpdateResumeAsync(Guid userId, IFormFile file, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id: userId, cancellationToken);

        if (findUser == null)
        {
            return Result<User>.Failure(new ErrorDetail("token", "user not found", 404));
        }

        var uploadResult = await mediaService.UploadFile(file, cancellationToken);

        if (uploadResult.IsFailure)
        {
            return Result<User>.Failure(uploadResult.Errors.ToArray());
        }

        if (findUser.WorkerProfile == null)
        {
            return Result<User>.Failure(new ErrorDetail("user", "Worker profile is null", 400));
        }

        findUser.WorkerProfile.SetResume(uploadResult.Value!);
        await workerRepository.UpdateAsync(findUser.WorkerProfile, cancellationToken);

        logger.LogInformation("Resume uploaded for {UserId}", userId);
        return Result<User>.Success(findUser);
    }

    public async Task<Result<User>> UpdateAvatarAsync(Guid userId, IFormFile file, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id: userId, cancellationToken);

        if (findUser == null)
        {
            return Result<User>.Failure(new ErrorDetail("token", "user not found", 404));
        }

        var uploadResult = await mediaService.UploadFile(file, cancellationToken);

        if (uploadResult.IsFailure)
        {
            return Result<User>.Failure(uploadResult.Errors.ToArray());
        }

        if (findUser.WorkerProfile == null)
        {
            return Result<User>.Failure(new ErrorDetail("user", "Worker profile is null", 400));
        }

        findUser.SetAvatar(uploadResult.Value!);
        await workerRepository.UpdateAsync(findUser.WorkerProfile, cancellationToken);
        await userService.UpdateAsync(findUser, cancellationToken);

        logger.LogInformation("Avatar uploaded for {UserId}", userId);
        return Result<User>.Success(findUser);
    }
}