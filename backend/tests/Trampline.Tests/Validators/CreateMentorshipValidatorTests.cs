using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class CreateMentorshipValidatorTests
{
    private readonly CreateMentorshipValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new CreateMentorshipRequest { Title = "React Mentorship", Description = "Описание", Address = "Москва" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Title_Fails(string? title)
    {
        var model = new CreateMentorshipRequest { Title = title!, Description = "D", Address = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_TooLong_Fails()
    {
        var model = new CreateMentorshipRequest { Title = new string('A', 201), Description = "D", Address = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_ExactlyMaxLength_Passes()
    {
        var model = new CreateMentorshipRequest { Title = new string('A', 200), Description = "D", Address = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Description_Fails(string? desc)
    {
        var model = new CreateMentorshipRequest { Title = "T", Description = desc!, Address = "A" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Address_Fails(string? address)
    {
        var model = new CreateMentorshipRequest { Title = "T", Description = "D", Address = address! };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void SalaryFrom_GreaterThan_SalaryTo_Fails()
    {
        var model = new CreateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Address = "A",
            SalaryFrom = 100000,
            SalaryTo = 50000
        };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.SalaryFrom);
    }

    [Fact]
    public void SalaryFrom_EqualTo_SalaryTo_Passes()
    {
        var model = new CreateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Address = "A",
            SalaryFrom = 50000,
            SalaryTo = 50000
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.SalaryFrom);
    }

    [Fact]
    public void SalaryFrom_LessThan_SalaryTo_Passes()
    {
        var model = new CreateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Address = "A",
            SalaryFrom = 30000,
            SalaryTo = 80000
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.SalaryFrom);
    }

    [Fact]
    public void SalaryFrom_Null_Passes()
    {
        var model = new CreateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Address = "A",
            SalaryFrom = null,
            SalaryTo = 80000
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.SalaryFrom);
    }

    [Fact]
    public void SalaryTo_Null_Passes()
    {
        var model = new CreateMentorshipRequest
        {
            Title = "T",
            Description = "D",
            Address = "A",
            SalaryFrom = 30000,
            SalaryTo = null
        };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.SalaryFrom);
    }
}
