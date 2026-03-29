using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models;

namespace Trampline.Contracts.Extensions;

public static class UserExtensions
{
    public static UserResponse ToUserResponse(this User user)
    {
        return new UserResponse()
        {
            Id = user.Id,
            Email = user.Email,
            Nickname = user.Nickname,
            Avatar = user.Avatar,
            Role = user.Role,
            IsSuperAdmin = user.IsSuperAdmin,
            EmployeeProfile = user.EmployeeProfile != null
                ? new EmployeeProfileResponse
                {
                    Id = user.EmployeeProfile.Id,
                    UserId = user.EmployeeProfile.UserId,
                    Activity = user.EmployeeProfile.Activity,
                    Description = user.EmployeeProfile.Description,
                    Name = user.EmployeeProfile.Name,
                    Photos = user.EmployeeProfile.Photos,
                    IsVerified = user.EmployeeProfile.IsVerified,
                    VerificationLevel = user.EmployeeProfile.VerificationLevel,
                    VerifiedName = user.EmployeeProfile.VerifiedName,
                    Link = user.EmployeeProfile.Link,
                    Socials = user.EmployeeProfile.Socials,
                    Videos = user.EmployeeProfile.Videos,
                    Info = user.EmployeeProfile.Info
                }
                : null
        };
    }
}