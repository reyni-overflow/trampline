using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models.Employee;

namespace Trampline.Contracts.Extensions;

public static class JobExtensions
{
    public static JobResponse ToJobResponse(this Job job)
    {
        return new JobResponse
        {
            Id = job.Id,
            Type = job.Type,
            EmployeeId = job.EmployeeId,
            UserId = job.UserId,
            Title = job.Title,
            Description = job.Description,
            Address = job.Address,
            City = job.City,
            Country = job.Country,
            CreatedAt = job.CreatedAt,
            UpdatedAt = job.UpdatedAt,
            DeletedAt = job.DeletedAt,
            EndedAt = job.EndedAt,
            IsActive = job.IsActive,
            IsPublished = job.IsPublished,
            Views = job.Views,
            Format = job.Format,
            SalaryFrom = job.SalaryFrom,
            GeoLat = job.GeoLat,
            GeoLon = job.GeoLon,
            Street = job.Street,
            SalaryTo = job.SalaryTo,
            Tags = job.Tags.Select(x => x.TagToResponse()).ToArray(),
            Photos = job.Photos,
            Videos = job.Videos,
            CompanyName = job.Employee?.Name ?? string.Empty
        };
    }

    public static TagResponse TagToResponse(this Tag tag)
    {
        return new TagResponse()
        {
            Id = tag.Id,
            Name = tag.Name,
            Category = tag.Category,
            Lvl = tag.Lvl
        };
    }

    public static JobApplicationResponse ToJobApplicationResponse(this JobApplication jobApplication)
    {
        return new JobApplicationResponse
        {
            Id = jobApplication.Id,
            CoverLetter = jobApplication.CoverLetter,
            JobId = jobApplication.JobId,
            JobTitle = jobApplication.Job?.Title,
            CompanyName = jobApplication.Job?.Employee?.Name,
            Profile = jobApplication.Profile.ToWorkerProfileResponse(),
            WorkerProfileId = jobApplication.WorkerProfileId,
            CreatedAt = jobApplication.CreatedAt,
            IsReadByEmployer = jobApplication.IsReadByEmployer,
            Status = jobApplication.Status,
        };
    }
}