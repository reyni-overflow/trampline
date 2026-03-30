using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models.Employee;

namespace Trampline.Contracts.Extensions;

public static class MentorshipExtensions
{
    public static MentorshipResponse ToMentorshipResponse(this Mentorship mentorship)
    {
        return new MentorshipResponse
        {
            Id = mentorship.Id,
            EmployeeId = mentorship.EmployeeId,
            UserId = mentorship.UserId,
            Title = mentorship.Title,
            Description = mentorship.Description,
            Address = mentorship.Address,
            City = mentorship.City,
            Country = mentorship.Country,
            CreatedAt = mentorship.CreatedAt,
            UpdatedAt = mentorship.UpdatedAt,
            DeletedAt = mentorship.DeletedAt,
            EndedAt = mentorship.EndedAt,
            StartDate = mentorship.StartDate,
            MaxParticipants = mentorship.MaxParticipants,
            Duration = mentorship.Duration,
            IsActive = mentorship.IsActive,
            IsPublished = mentorship.IsPublished,
            Views = mentorship.Views,
            Format = mentorship.Format,
            SalaryFrom = mentorship.SalaryFrom,
            SalaryTo = mentorship.SalaryTo,
            Tags = mentorship.Tags.Select(x => x.TagToResponse()).ToArray(),
            Photos = mentorship.Photos,
            Videos = mentorship.Videos,
            GeoLat = mentorship.GeoLat,
            GeoLon = mentorship.GeoLon,
            Street = mentorship.Street,
            CompanyName = mentorship.Profile?.Name ?? string.Empty
        };
    }

    public static MentorshipApplicationResponse ToMentorshipApplicationResponse(this MentorshipApplication mentorshipApplication)
    {
        return new MentorshipApplicationResponse
        {
            Id = mentorshipApplication.Id,
            CoverLetter = mentorshipApplication.CoverLetter ?? string.Empty,
            MentorshipId = mentorshipApplication.MentorshipId,
            MentorshipTitle = mentorshipApplication.Mentorship?.Title,
            CompanyName = mentorshipApplication.Mentorship?.Profile?.Name,
            Profile = mentorshipApplication.Profile.ToWorkerProfileResponse(),
            WorkerProfileId = mentorshipApplication.WorkerProfileId,
            CreatedAt = mentorshipApplication.CreatedAt,
            IsReadByEmployer = mentorshipApplication.IsReadByEmployer,
            Status = mentorshipApplication.Status,
        };
    }
}
