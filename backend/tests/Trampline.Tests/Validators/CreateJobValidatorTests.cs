using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class CreateJobValidatorTests
{
    private readonly CreateJobValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new CreateJobRequest { Title = "Backend Developer", Description = "Описание", Address = "Москва" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Title_Fails(string? title)
    {
        var model = new CreateJobRequest { Title = title!, Description = "D", Address = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Description_Fails(string? desc)
    {
        var model = new CreateJobRequest { Title = "T", Description = desc!, Address = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Address_Fails(string? address)
    {
        var model = new CreateJobRequest { Title = "T", Description = "D", Address = address! };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }
}
