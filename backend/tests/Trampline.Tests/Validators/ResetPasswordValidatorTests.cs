using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class ResetPasswordValidatorTests
{
    private readonly ResetPasswordValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new ResetPasswordRequest { Email = "test@example.com", Code = "123456", NewPassword = "NewPass1" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Fails()
    {
        var model = new ResetPasswordRequest { Email = "", Code = "123456", NewPassword = "NewPass1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Invalid_Email_Fails()
    {
        var model = new ResetPasswordRequest { Email = "invalid", Code = "123456", NewPassword = "NewPass1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Empty_Code_Fails()
    {
        var model = new ResetPasswordRequest { Email = "test@test.com", Code = "", NewPassword = "NewPass1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Short_Code_Fails()
    {
        var model = new ResetPasswordRequest { Email = "test@test.com", Code = "12345", NewPassword = "NewPass1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Long_Code_Fails()
    {
        var model = new ResetPasswordRequest { Email = "test@test.com", Code = "1234567", NewPassword = "NewPass1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void Weak_NewPassword_Fails()
    {
        var model = new ResetPasswordRequest { Email = "test@test.com", Code = "123456", NewPassword = "weak" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }
}
