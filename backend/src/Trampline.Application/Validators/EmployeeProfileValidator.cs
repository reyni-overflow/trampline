using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class EmployeeProfileValidator : AbstractValidator<EmployeeProfileRequest>
{
    public EmployeeProfileValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Name is required")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters");
        RuleFor(r => r.Description).NotEmpty().WithMessage("Description is required")
            .MaximumLength(10000).WithMessage("Description must be at most 10000 characters");
        RuleFor(r => r.Activity).NotEmpty().WithMessage("Activity is required")
            .MaximumLength(200).WithMessage("Activity must be at most 200 characters");
    }
}
