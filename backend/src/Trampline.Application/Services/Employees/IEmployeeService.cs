using Microsoft.AspNetCore.Http;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;
using Trampline.Core.Models.Employee;
using Trampline.Shared.Results;

namespace Trampline.Application.Services.Employees;

public interface IEmployeeService
{
    Task<Result<User>> UpdateVideoAsync(Guid userId, IFormFile[] files, CancellationToken cancellationToken);

    Task<Result<User>> UpdatePhotosAsync(Guid userId, IFormFile[] files, CancellationToken cancellationToken);

    Task<Result<User>> UpdateProfileAsync(Guid userId, EmployeeProfileRequest request, CancellationToken cancellationToken);

    Task<Result<FindResponse>> VerifyCompanyAsync(Guid userId, CancellationToken cancellationToken);

    Task<Result<User>> UpdateInfoAsync(Guid userId, EmployeeInfo info, CancellationToken cancellationToken);
}