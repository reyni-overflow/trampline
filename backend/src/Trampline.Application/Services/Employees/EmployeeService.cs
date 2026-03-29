using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trampline.Application.Services.IO;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Core.Repositories;
using Trampline.Application.Utils;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.Employees;

public class EmployeeService(
    IEmployeeRepository employeeRepository,
    IUserService userService,
    ILogger<EmployeeService> logger,
    IMediaService mediaService,
    IDaDataService daDataService) : IEmployeeService
{
    public async Task<Result<User>> UpdateVideoAsync(Guid userId, IFormFile[] files, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id: userId, cancellationToken);

        if (findUser == null || findUser.EmployeeProfile == null)
        {
            return Result<User>.Failure(new ErrorDetail("token", "user not found", 404));
        }

        if (files.Length > 0)
        {
            foreach (var file in files)
            {
                var result = await mediaService.UploadFile(file, cancellationToken);

                if (result.IsSuccess)
                {
                    findUser.EmployeeProfile.AddVideo(result.Value!);
                }
            }

        }

        await userService.UpdateAsync(findUser, cancellationToken);
        logger.LogInformation("Videos uploaded for {UserId}, count: {Count}", userId, files.Length);
        return Result<User>.Success(findUser);
    }

    public async Task<Result<User>> UpdatePhotosAsync(Guid userId, IFormFile[] files, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id: userId, cancellationToken);

        if (findUser == null || findUser.EmployeeProfile == null)
        {
            return Result<User>.Failure(new ErrorDetail("token", "user not found", 404));
        }

        if (files.Length > 0)
        {
            foreach (var file in files)
            {
                var result = await mediaService.UploadFile(file, cancellationToken);

                if (result.IsSuccess)
                {
                    findUser.EmployeeProfile.AddPhoto(result.Value!);
                }
            }

        }

        await userService.UpdateAsync(findUser, cancellationToken);
        logger.LogInformation("Photos uploaded for {UserId}, count: {Count}", userId, files.Length);
        return Result<User>.Success(findUser);
    }

    public async Task<Result<User>> UpdateProfileAsync(Guid userId, EmployeeProfileRequest request, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id: userId, cancellationToken);

        if (findUser == null)
        {
            return Result<User>.Failure(new ErrorDetail("token", "user not found", 404));
        }

        var name = HtmlSanitization.Sanitize(request.Name);
        var description = HtmlSanitization.Sanitize(request.Description);
        var activity = HtmlSanitization.Sanitize(request.Activity);

        if (findUser.EmployeeProfile == null)
        {
            var profile = EmployeeProfile.Create(userId, name, description,
                activity, request.Info, request.Link);
            findUser.SetEmployeeProfile(profile);
            profile.UpdateInfo(request.Info);
            await employeeRepository.AddAsync(profile, cancellationToken);
        }
        else
        {
            var nameChanged = findUser.EmployeeProfile.VerificationLevel >= 1
                && findUser.EmployeeProfile.IsNameChangeSignificant(name);
            findUser.EmployeeProfile.Update(name, description, activity, request.Link);
            findUser.EmployeeProfile.UpdateInfo(request.Info);
            if (nameChanged)
                findUser.EmployeeProfile.Unverify();
            await employeeRepository.UpdateAsync(findUser.EmployeeProfile, cancellationToken);
        }

        await userService.UpdateAsync(findUser, cancellationToken);
        logger.LogInformation("Employee profile updated for {UserId}", userId);
        return Result<User>.Success(findUser);
    }

    public async Task<Result<FindResponse>> VerifyCompanyAsync(Guid userId, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id: userId, cancellationToken);

        if (findUser == null || findUser.EmployeeProfile == null)
        {
            return Result<FindResponse>.Failure(new ErrorDetail("id", "user not found", 404));
        }

        if (string.IsNullOrWhiteSpace(findUser.EmployeeProfile.Info.INN))
        {
            return Result<FindResponse>.Failure(new ErrorDetail("inn", "ИНН обязателен для верификации", 400));
        }

        var result = await daDataService.FindParty(findUser.EmployeeProfile.Info.INN, cancellationToken);

        if (result.IsFailure)
        {
            return Result<FindResponse>.Failure(result.Errors.ToArray());
        }

        var verifiedName = result.Value!.Value;
        findUser.EmployeeProfile.SetVerificationLevel(1);
        findUser.EmployeeProfile.SetVerifiedName(verifiedName);
        if (!string.IsNullOrWhiteSpace(verifiedName))
            findUser.EmployeeProfile.Update(verifiedName, findUser.EmployeeProfile.Description, findUser.EmployeeProfile.Activity, findUser.EmployeeProfile.Link);
        await employeeRepository.UpdateAsync(findUser.EmployeeProfile, cancellationToken);

        logger.LogInformation("Company verification initiated for {UserId}, name set to '{Name}'", userId, verifiedName);
        return Result<FindResponse>.Success(result.Value!);
    }

    public async Task<Result<User>> UpdateInfoAsync(Guid userId, EmployeeInfo info, CancellationToken cancellationToken)
    {
        var findUser = await userService.GetByIdAsync(id: userId, cancellationToken);

        if (findUser == null || findUser.EmployeeProfile == null)
        {
            return Result<User>.Failure(new ErrorDetail("token", "employee not found", 404));
        }

        findUser.EmployeeProfile.UpdateInfo(info);
        await employeeRepository.UpdateAsync(findUser.EmployeeProfile, cancellationToken);

        logger.LogInformation("Employee info updated for {UserId}", userId);
        return Result<User>.Success(findUser);
    }
}