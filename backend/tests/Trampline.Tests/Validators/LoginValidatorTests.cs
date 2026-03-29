using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new LoginRequest { Contact = "test@example.com", Password = "Password123" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Contact_FailsValidation()
    {
        var model = new LoginRequest { Contact = "", Password = "Password123" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Contact);
    }

    [Fact]
    public void Empty_Password_FailsValidation()
    {
        var model = new LoginRequest { Contact = "test@test.com", Password = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("+79001234567")]
    [InlineData("9001234567")]
    public void Various_ContactFormats_PassValidation(string contact)
    {
        var model = new LoginRequest { Contact = contact, Password = "Password123" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Contact);
    }
}
