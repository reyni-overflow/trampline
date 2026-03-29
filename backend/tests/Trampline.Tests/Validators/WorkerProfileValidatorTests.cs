using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;

namespace Trampline.Tests.Validators;

public class WorkerProfileValidatorTests
{
    private readonly WorkerProfileValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var model = new WorkerProfileRequest { Name = "Иван", LastName = "Иванов", Patronymic = "Иванович" };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_Name_Fails(string? name)
    {
        var model = new WorkerProfileRequest { Name = name!, LastName = "Last", Patronymic = "Pat" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_LastName_Fails(string? lastName)
    {
        var model = new WorkerProfileRequest { Name = "Name", LastName = lastName!, Patronymic = "Pat" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }
}
