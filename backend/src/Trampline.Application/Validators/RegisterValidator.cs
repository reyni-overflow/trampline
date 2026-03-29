using FluentValidation;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;

namespace Trampline.Application.Validators;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator()
    {
        RuleFor(r => r.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(r => r.Name).NotEmpty().WithMessage("Name is required");
        RuleFor(r => r.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit");
        RuleFor(r => r.Role)
            .Must(r => r == Role.Worker || r == Role.Employee)
            .WithMessage("Role must be Worker or Employee");
    }
}