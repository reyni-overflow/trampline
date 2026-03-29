using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class CreateEventValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventValidator()
    {
        RuleFor(r => r.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must be at most 200 characters");

        RuleFor(r => r.Description)
            .NotEmpty()
            .WithMessage("Description is required");

        RuleFor(r => r.Address)
            .NotEmpty()
            .WithMessage("Address is required");

        RuleFor(r => r.SalaryFrom)
            .Must((r, salaryFrom) => salaryFrom <= r.SalaryTo)
            .WithMessage("SalaryFrom must be less than or equal to SalaryTo")
            .When(r => r.SalaryFrom is not null && r.SalaryTo is not null);
    }
}
