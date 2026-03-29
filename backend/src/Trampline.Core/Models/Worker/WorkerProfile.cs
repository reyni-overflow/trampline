using System.ComponentModel.DataAnnotations;
using Trampline.Core.Exceptions;
using Trampline.Core.Models.Employee;

namespace Trampline.Core.Models.Worker;

public class WorkerProfile
{
    [Key]
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public User User { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public string Patronymic { get; private set; } = string.Empty;

    public WorkerInfo? Info { get; private set; }

    public string? About { get; private set; }

    public string? Photo { get; private set; }

    public string? Resume { get; private set; }

    public List<string> Skills { get; private set; } = [];

    public List<string> Repos { get; private set; } = [];

    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();

    public ICollection<EventApplication> EventApplications { get; set; } = new List<EventApplication>();

    public ICollection<MentorshipApplication> MentorshipApplications { get; set; } = new List<MentorshipApplication>();

    private WorkerProfile() { }

    public static WorkerProfile Create(Guid userId, string name, string lastName,
        string patronymic, WorkerInfo? info, string? about, string? photo)
    {
        return new WorkerProfile()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            Patronymic = patronymic,
            LastName = lastName,
            About = about,
            Photo = photo,
            Info = info
        };
    }

    public void Update(string name, string lastName,
        string patronymic, string? about, string? photo)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Name is required");
        }

        if (name.Length < 2)
        {
            throw new DomainException("Name must be at least 2 characters");
        }

        if (string.IsNullOrWhiteSpace(lastName))
        {
            throw new DomainException("LastName is required");
        }

        if (string.IsNullOrWhiteSpace(patronymic))
        {
            throw new DomainException("Patronymic is required");
        }

        Name = name.Trim();
        LastName = lastName.Trim();
        Patronymic = patronymic.Trim();
        About = about?.Trim();
        Photo = photo?.Trim();
    }

    public void SetResume(string? resume)
    {
        if (string.IsNullOrWhiteSpace(resume))
        {
            throw new DomainException("Resume is required");
        }

        Resume = resume.Trim();
    }

    public void UpdateInfo(WorkerInfo info) => Info = info;

    public void UpdateSkills(List<string> skills) => Skills = skills;

    public void UpdateRepos(List<string> repos) => Repos = repos;
}