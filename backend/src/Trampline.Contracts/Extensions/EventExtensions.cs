using Trampline.Contracts.DTOs.Responses;
using Trampline.Core.Models.Employee;

namespace Trampline.Contracts.Extensions;

public static class EventExtensions
{
    public static EventResponse ToEventResponse(this Event evt)
    {
        return new EventResponse
        {
            Id = evt.Id,
            EmployeeId = evt.EmployeeId,
            UserId = evt.UserId,
            Title = evt.Title,
            Description = evt.Description,
            Address = evt.Address,
            City = evt.City,
            Country = evt.Country,
            CreatedAt = evt.CreatedAt,
            UpdatedAt = evt.UpdatedAt,
            DeletedAt = evt.DeletedAt,
            EndedAt = evt.EndedAt,
            StartDate = evt.StartDate,
            IsActive = evt.IsActive,
            IsPublished = evt.IsPublished,
            Views = evt.Views,
            Format = evt.Format,
            SalaryFrom = evt.SalaryFrom,
            SalaryTo = evt.SalaryTo,
            Tags = evt.Tags.Select(x => x.TagToResponse()).ToArray(),
            Photos = evt.Photos,
            Videos = evt.Videos,
            GeoLat = evt.GeoLat,
            GeoLon = evt.GeoLon,
            Street = evt.Street,
            CompanyName = evt.Profile?.Name ?? string.Empty
        };
    }

    public static EventApplicationResponse ToEventApplicationResponse(this EventApplication eventApplication)
    {
        return new EventApplicationResponse
        {
            Id = eventApplication.Id,
            CoverLetter = eventApplication.CoverLetter ?? string.Empty,
            EventId = eventApplication.EventId,
            EventTitle = eventApplication.Event?.Title,
            CompanyName = eventApplication.Event?.Profile?.Name,
            Profile = eventApplication.Profile.ToWorkerProfileResponse(),
            WorkerProfileId = eventApplication.WorkerProfileId,
            CreatedAt = eventApplication.CreatedAt,
            IsReadByEmployer = eventApplication.IsReadByEmployer,
            Status = eventApplication.Status,
        };
    }
}
