using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class CreateEventValidatorTests
{
    private readonly CreateEventValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new CreateEventRequest { Title = "Хакатон 2026", Description = "Описание мероприятия", Address = "Москва" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Title_Fails()
    {
        var model = new CreateEventRequest { Title = "", Description = "Desc", Address = "Addr" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void TooLong_Title_Fails()
    {
        var model = new CreateEventRequest { Title = new string('A', 201), Description = "Desc", Address = "Addr" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void MaxLength_Title_Passes()
    {
        var model = new CreateEventRequest { Title = new string('A', 200), Description = "Desc", Address = "Addr" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Empty_Description_Fails()
    {
        var model = new CreateEventRequest { Title = "Title", Description = "", Address = "Addr" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Empty_Address_Fails()
    {
        var model = new CreateEventRequest { Title = "Title", Description = "Desc", Address = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }
}
