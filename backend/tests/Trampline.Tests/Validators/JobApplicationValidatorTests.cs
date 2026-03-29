using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class JobApplicationValidatorTests
{
    private readonly JobApplicationValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new JobApplicationRequest { JobId = Guid.NewGuid(), CoverLetter = new string('A', 60) };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_JobId_Fails()
    {
        var model = new JobApplicationRequest { JobId = Guid.Empty, CoverLetter = new string('A', 60) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.JobId);
    }

    [Fact]
    public void Empty_CoverLetter_Fails()
    {
        var model = new JobApplicationRequest { JobId = Guid.NewGuid(), CoverLetter = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter);
    }

    [Fact]
    public void Short_CoverLetter_Fails()
    {
        var model = new JobApplicationRequest { JobId = Guid.NewGuid(), CoverLetter = "Short" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter);
    }

    [Fact]
    public void TooLong_CoverLetter_Fails()
    {
        var model = new JobApplicationRequest { JobId = Guid.NewGuid(), CoverLetter = new string('A', 5001) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter);
    }

    [Fact]
    public void ExactMinLength_CoverLetter_Passes()
    {
        var model = new JobApplicationRequest { JobId = Guid.NewGuid(), CoverLetter = new string('A', 50) };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.CoverLetter);
    }

    [Fact]
    public void ExactMaxLength_CoverLetter_Passes()
    {
        var model = new JobApplicationRequest { JobId = Guid.NewGuid(), CoverLetter = new string('A', 2000) };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.CoverLetter);
    }
}
