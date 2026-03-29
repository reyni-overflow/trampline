using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new RegisterRequest { Email = "test@example.com", Name = "TestUser", Password = "Password1" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Email_FailsValidation(string? email)
    {
        var model = new RegisterRequest { Email = email!, Name = "Test", Password = "Password1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Name_FailsValidation(string? name)
    {
        var model = new RegisterRequest { Email = "test@test.com", Name = name!, Password = "Password1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Password_FailsValidation(string? password)
    {
        var model = new RegisterRequest { Email = "test@test.com", Name = "Test", Password = password! };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Short_Password_FailsValidation()
    {
        var model = new RegisterRequest { Email = "test@test.com", Name = "Test", Password = "Pass1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters");
    }

    [Fact]
    public void NoUppercase_Password_FailsValidation()
    {
        var model = new RegisterRequest { Email = "test@test.com", Name = "Test", Password = "password1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter");
    }

    [Fact]
    public void NoLowercase_Password_FailsValidation()
    {
        var model = new RegisterRequest { Email = "test@test.com", Name = "Test", Password = "PASSWORD1" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter");
    }

    [Fact]
    public void NoDigit_Password_FailsValidation()
    {
        var model = new RegisterRequest { Email = "test@test.com", Name = "Test", Password = "Passwords" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one digit");
    }

    [Theory]
    [InlineData("StrongP1")]
    [InlineData("MyPass123")]
    public void Valid_Passwords_PassValidation(string password)
    {
        var model = new RegisterRequest { Email = "test@test.com", Name = "Test", Password = password };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void CyrillicPassword_WithDigitAndUppercase_MayHaveSpecificBehavior()
    {
        var model = new RegisterRequest { Email = "test@test.com", Name = "Test", Password = "Пароль123A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
