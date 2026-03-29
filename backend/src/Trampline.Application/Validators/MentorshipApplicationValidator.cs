using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class MentorshipApplicationValidator : AbstractValidator<MentorshipApplicationRequest>
{
    public MentorshipApplicationValidator()
    {
        RuleFor(r => r.MentorshipId)
            .NotEmpty()
            .WithMessage("MentorshipId is required");

        RuleFor(r => r.CoverLetter)
            .NotEmpty()
            .WithMessage("CoverLetter is required")
            .Length(50, 5000)
            .WithMessage("CoverLetter must be between 50 and 5000 characters");
    }
}
