using System.ComponentModel.DataAnnotations;
using Trampline.Core.Exceptions;

namespace Trampline.Core.Models.Employee;

public class Mentorship
{
    [Key]
    public Guid Id { get; init; }

    public Guid EmployeeId { get; private set; }

    public Guid UserId { get; private set; }

    public EmployeeProfile Profile { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Address { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string Street { get; private set; } = string.Empty;

    public double GeoLon { get; private set; }

    public double GeoLat { get; private set; }

    public string Country { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public DateTime? DeletedAt { get; private set; }

    public DateTime EndedAt { get; private set; } = DateTime.UtcNow.AddDays(31);

    public DateTime? StartDate { get; private set; }

    public int? MaxParticipants { get; private set; }

    public string? Duration { get; private set; }

    public bool IsActive { get; private set; } = true;

    public bool IsPublished { get; private set; } = true;

    public int Views => UserViews.Count;

    private HashSet<Guid> _userViews { get; set; } = new();

    public IReadOnlyCollection<Guid> UserViews => _userViews;

    public WorkFormat Format { get; private set; } = WorkFormat.Hybrid;

    public decimal? SalaryFrom { get; private set; }

    public decimal? SalaryTo { get; private set; }

    private List<Tag> _tags { get; set; } = new();

    public IReadOnlyCollection<Tag> Tags => _tags;

    public List<MentorshipApplication> MentorshipApplications { get; set; } = new();

    public List<string> Photos { get; private set; } = new();

    public List<string> Videos { get; private set; } = new();

    public List<string> CustomTags { get; private set; } = new();

    private Mentorship() { }

    public static Mentorship Create(Guid employeeId, Guid userId, string title, string description, WorkFormat format)
    {
        return new Mentorship()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EmployeeId = employeeId,
            Title = title,
            Description = description,
            Format = format,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateGeo(string address,
        string city, string country, string street, double lat, double lon)
    {
        if ((lat < -90 || lat > 90))
            throw new DomainException("Invalid latitude");
        if ((lon < -180 || lon > 180))
            throw new DomainException("Invalid longitude");

        Street = street;
        Address = address;
        City = city;
        Country = country;
        GeoLat = lat;
        GeoLon = lon;
    }

    public void AddViews(Guid userId)
    {
        if (EmployeeId != userId)
        {
            _userViews.Add(userId);
        }
    }

    public void UpdateSalary(decimal? salaryFrom, decimal? salaryTo)
    {
        SalaryFrom = salaryFrom.HasValue ? Math.Clamp(salaryFrom.Value, 0, 1_500_000) : null;
        SalaryTo = salaryTo.HasValue ? Math.Clamp(salaryTo.Value, 0, 1_500_000) : null;
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return;

        var normalized = tag.Trim();

        if (_tags.Any(x => string.Equals(x.Name, normalized, StringComparison.OrdinalIgnoreCase)))
            return;

        _tags.Add(new Tag
        {
            Id = Guid.NewGuid(),
            Name = normalized
        });
    }

    public void AddTag(params Tag[] tags)
    {
        foreach (var tag in tags)
        {
            if (_tags.Any(x => x.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
                continue;

            _tags.Add(tag);
        }
    }

    public void UpdateTags(params Tag[] tags)
    {
        _tags.Clear();

        _tags.AddRange(tags);
    }

    public void AddPhoto(string path) => Photos.Add(path);

    public void AddVideo(string path) => Videos.Add(path);

    public void UpdateStartDate(DateTime? startDate) => StartDate = startDate;

    public void UpdateMaxParticipants(int? maxParticipants) => MaxParticipants = maxParticipants;

    public void UpdateDuration(string? duration) => Duration = duration;

    public void UpdateEndedAt(DateTime? endedAt)
    {
        if (endedAt.HasValue)
            EndedAt = endedAt.Value;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }

    public void SetActive(bool active) => IsActive = active;

    public void SetPublished(bool published) => IsPublished = published;

    public void SetFormat(WorkFormat format) => Format = format;

    public void SetCustomTags(List<string> tags) => CustomTags = tags ?? new();

    public void Update(string title, string description, string address,
        string city, string country)
    {
        UpdatedAt = DateTime.UtcNow;
        Title = title;
        Description = description;
        Address = address;
        City = city;
        Country = country;
    }
}
