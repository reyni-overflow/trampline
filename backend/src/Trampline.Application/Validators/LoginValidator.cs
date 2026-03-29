using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(r => r.Contact).NotEmpty().WithMessage("Contact is required");
        RuleFor(r => r.Password).NotEmpty().WithMessage("Password is required");
    }
}