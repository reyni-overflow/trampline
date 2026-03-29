using System.ComponentModel.DataAnnotations;

namespace Trampline.Core.Models.Employee;

public class Tag
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public int Lvl { get; set; }

    public List<Job> Jobs { get; set; } = new();

    public List<Event> Events { get; set; } = new();

    public List<Mentorship> Mentorships { get; set; } = new();
}