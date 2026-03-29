using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class JobApplicationValidator : AbstractValidator<JobApplicationRequest>
{
    public JobApplicationValidator()
    {
        RuleFor(r => r.JobId)
            .NotEmpty()
            .WithMessage("JobId is required");

        RuleFor(r => r.CoverLetter)
            .NotEmpty()
            .WithMessage("CoverLetter is required")
            .Length(50, 5000)
            .WithMessage("CoverLetter must be between 50 and 5000 characters");
    }
}