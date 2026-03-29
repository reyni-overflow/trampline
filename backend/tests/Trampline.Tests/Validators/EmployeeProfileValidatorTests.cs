using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class EmployeeProfileValidatorTests
{
    private readonly EmployeeProfileValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new EmployeeProfileRequest { Name = "CompanyName", Description = "Описание компании", Activity = "IT" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Name_Fails(string? name)
    {
        var model = new EmployeeProfileRequest { Name = name!, Description = "D", Activity = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Short_Name_Fails()
    {
        var model = new EmployeeProfileRequest { Name = "A", Description = "D", Activity = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Empty_Description_Fails()
    {
        var model = new EmployeeProfileRequest { Name = "Company", Description = "", Activity = "IT" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Empty_Activity_Fails()
    {
        var model = new EmployeeProfileRequest { Name = "Company", Description = "Desc", Activity = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Activity);
    }
}
