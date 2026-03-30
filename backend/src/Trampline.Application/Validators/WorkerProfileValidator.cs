using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class WorkerProfileValidator : AbstractValidator<WorkerProfileRequest>
{
    public WorkerProfileValidator()
    {
        RuleFor(r => r.Name).NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be at most 100 characters");
        RuleFor(r => r.LastName).NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must be at most 100 characters");
        RuleFor(r => r.About).MaximumLength(5000).WithMessage("About must be at most 5000 characters");
    }
}
