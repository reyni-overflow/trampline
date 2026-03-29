using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class DeleteAccountValidatorTests
{
    private readonly DeleteAccountValidator _validator = new();

    [Fact]
    public void Valid_Password_PassesValidation()
    {
        var model = new DeleteAccountRequest { Password = "MyPassword123" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Password_Fails()
    {
        var model = new DeleteAccountRequest { Password = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
