using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class UpdateMentorshipValidator : AbstractValidator<UpdateMentorshipRequest>
{
    public UpdateMentorshipValidator()
    {
        RuleFor(r => r.Title)
            .NotEmpty().WithMessage("Title must not be empty")
            .MaximumLength(200).WithMessage("Title must be at most 200 characters")
            .When(r => r.Title is not null);
        RuleFor(r => r.Description)
            .NotEmpty().WithMessage("Description must not be empty")
            .MaximumLength(10000).WithMessage("Description must be at most 10000 characters")
            .When(r => r.Description is not null);
        RuleFor(r => r.Address)
            .MaximumLength(500).WithMessage("Address must be at most 500 characters")
            .When(r => r.Address is not null);

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
