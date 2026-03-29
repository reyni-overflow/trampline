using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class EventApplicationValidator : AbstractValidator<EventApplicationRequest>
{
    public EventApplicationValidator()
    {
        RuleFor(r => r.EventId)
            .NotEmpty()
            .WithMessage("EventId is required");

        RuleFor(r => r.CoverLetter)
            .NotEmpty()
            .WithMessage("CoverLetter is required")
            .Length(50, 5000)
            .WithMessage("CoverLetter must be between 50 and 5000 characters");
    }
}
