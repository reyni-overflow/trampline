using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class ForgotPasswordValidatorTests
{
    private readonly ForgotPasswordValidator _validator = new();

    [Fact]
    public void Valid_Email_PassesValidation()
    {
        var model = new ForgotPasswordRequest { Email = "test@example.com" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Fails()
    {
        var model = new ForgotPasswordRequest { Email = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Invalid_Email_Fails()
    {
        var model = new ForgotPasswordRequest { Email = "notanemail" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
