using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class EventApplicationValidatorTests
{
    private readonly EventApplicationValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new EventApplicationRequest { EventId = Guid.NewGuid(), CoverLetter = new string('A', 60) };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_EventId_Fails()
    {
        var model = new EventApplicationRequest { EventId = Guid.Empty, CoverLetter = new string('A', 60) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.EventId);
    }

    [Fact]
    public void Empty_CoverLetter_Fails()
    {
        var model = new EventApplicationRequest { EventId = Guid.NewGuid(), CoverLetter = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter);
    }

    [Fact]
    public void Short_CoverLetter_Fails()
    {
        var model = new EventApplicationRequest { EventId = Guid.NewGuid(), CoverLetter = "Short text" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter);
    }

    [Fact]
    public void TooLong_CoverLetter_Fails()
    {
        var model = new EventApplicationRequest { EventId = Guid.NewGuid(), CoverLetter = new string('A', 5001) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter);
    }
}
