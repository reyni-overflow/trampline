using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class WorkerProfileValidator : AbstractValidator<WorkerProfileRequest>
{
    public WorkerProfileValidator()
    {
        RuleFor(r => r.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(r => r.LastName).NotEmpty().WithMessage("Last name is required");
    }
}
