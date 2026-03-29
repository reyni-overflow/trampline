using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class DeleteAccountValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountValidator()
    {
        RuleFor(r => r.Password).NotEmpty().WithMessage("Password is required");
    }
}
