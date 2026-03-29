namespace Trampline.Core.Models.Employee;

public record EmployeeInfo
{
    public string Address { get; set; } = string.Empty;

    public string INN { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}