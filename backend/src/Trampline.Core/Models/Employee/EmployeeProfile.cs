using System.ComponentModel.DataAnnotations;
using Trampline.Core.Exceptions;

namespace Trampline.Core.Models.Employee;

public class EmployeeProfile
{
    [Key]
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Activity { get; private set; } = string.Empty;

    public string? Link { get; private set; }

    public List<string> Socials { get; private set; } = new();

    public List<string> Photos { get; private set; } = new();

    public List<string> Videos { get; private set; } = new();

    public List<Job> Jobs { get; set; } = new();

    public List<Event> Events { get; set; } = new();

    public List<Mentorship> Mentorships { get; set; } = new();

    public int VerificationLevel { get; private set; } = 0;

    public string? VerifiedName { get; private set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsVerified => VerificationLevel >= 1;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsTrusted => VerificationLevel >= 2;

    public EmployeeInfo Info { get; private set; } = null!;

    private EmployeeProfile() { }

    public static EmployeeProfile Create(Guid userId, string name, string description,
        string activity, EmployeeInfo info, string? link)
    {
        return new EmployeeProfile()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Activity = activity,
            Description = description,
            Link = link,
            Info = info,
        };
    }

    public void Update(string name, string description, string activity, string? link)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required");

        if (name.Length < 2)
            throw new DomainException("Name must be at least 2 characters");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required");

        if (string.IsNullOrWhiteSpace(activity))
            throw new DomainException("Activity is required");

        Name = name.Trim();
        Description = description.Trim();
        Activity = activity.Trim();
        Link = link?.Trim();
    }

    public void UpdateSocials(List<string> socials)
    {
        Socials = socials ?? new List<string>();
    }

    public void AddPhoto(string photo)
    {
        Photos.Add(photo);
    }

    public void AddVideo(string video)
    {
        Videos.Add(video);
    }

    public void Verify() => VerificationLevel = 1;

    public void Unverify() => VerificationLevel = 0;

    public void SetVerified(bool verified) => VerificationLevel = verified ? 2 : 0;

    public void SetVerificationLevel(int level) => VerificationLevel = level;

    public void SetVerifiedName(string name) => VerifiedName = name;

    public bool IsNameChangeSignificant(string newName)
    {
        if (string.IsNullOrWhiteSpace(VerifiedName)) return false;

        var normalizedNew = newName.Trim();
        var normalizedVerified = VerifiedName.Trim();

        if (string.Equals(normalizedNew, Name, StringComparison.OrdinalIgnoreCase))
            return false;

        if (normalizedVerified.Contains(normalizedNew, StringComparison.OrdinalIgnoreCase))
            return false;

        if (normalizedNew.Contains(normalizedVerified, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    public void UpdateInfo(EmployeeInfo info)
    {
        Info = info;
    }
}