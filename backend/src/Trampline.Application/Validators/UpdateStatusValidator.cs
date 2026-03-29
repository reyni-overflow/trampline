using FluentValidation;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Application.Validators;

public class UpdateStatusValidator : AbstractValidator<UpdateStatusRequest>
{
    public UpdateStatusValidator()
    {
        RuleFor(r => r.Status).IsInEnum().WithMessage("Invalid status value");
    }
}
