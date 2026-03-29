using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class ChangePasswordValidatorTests
{
    private readonly ChangePasswordValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new ChangePasswordRequest { CurrentPassword = "OldPass123", NewPassword = "NewPass123" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_CurrentPassword_Fails()
    {
        var model = new ChangePasswordRequest { CurrentPassword = "", NewPassword = "NewPass123" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Empty_NewPassword_Fails()
    {
        var model = new ChangePasswordRequest { CurrentPassword = "OldPass123", NewPassword = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Short_NewPassword_Fails()
    {
        var model = new ChangePasswordRequest { CurrentPassword = "OldPass123", NewPassword = "Short1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NoUppercase_NewPassword_Fails()
    {
        var model = new ChangePasswordRequest { CurrentPassword = "OldPass123", NewPassword = "newpass123" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void NoDigit_NewPassword_Fails()
    {
        var model = new ChangePasswordRequest { CurrentPassword = "OldPass123", NewPassword = "NewPassWord" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
