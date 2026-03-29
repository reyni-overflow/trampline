using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class EmployeeProfileValidator : AbstractValidator<EmployeeProfileRequest>
{
    public EmployeeProfileValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters");
        RuleFor(r => r.Description).NotEmpty().WithMessage("Description is required");
        RuleFor(r => r.Activity).NotEmpty().WithMessage("Activity is required");
    }
}
