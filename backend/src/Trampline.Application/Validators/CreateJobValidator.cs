using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class CreateJobValidator : AbstractValidator<CreateJobRequest>
{
    public CreateJobValidator()
    {
        RuleFor(r => r.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters");
        RuleFor(r => r.Address)
            .NotEmpty().WithMessage("Address is required")
            .MaximumLength(500).WithMessage("Address must be at most 500 characters");
        RuleFor(r => r.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(10000).WithMessage("Description must be at most 10000 characters");

        RuleFor(r => r.SalaryFrom)
            .GreaterThanOrEqualTo(0).WithMessage("SalaryFrom must be non-negative")
            .When(r => r.SalaryFrom is not null);
        RuleFor(r => r.SalaryTo)
            .GreaterThanOrEqualTo(0).WithMessage("SalaryTo must be non-negative")
            .When(r => r.SalaryTo is not null);
        RuleFor(r => r.SalaryFrom)
            .Must((r, salaryFrom) => salaryFrom <= r.SalaryTo)
            .WithMessage("SalaryFrom must be less than or equal to SalaryTo")
            .When(r => r.SalaryFrom is not null && r.SalaryTo is not null);
    }
}