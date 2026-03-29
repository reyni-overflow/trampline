using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class CreateTagValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(50).WithMessage("Tag name must be at most 50 characters")
            .Matches(@"^[\w\s\-\.\#\+]+$").WithMessage("Tag name contains invalid characters");
        RuleFor(r => r.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(50).WithMessage("Category must be at most 50 characters");
    }
}
