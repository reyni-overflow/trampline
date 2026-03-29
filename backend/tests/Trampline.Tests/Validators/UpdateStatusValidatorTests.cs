using FluentValidation.TestHelper;
using Trampline.Application.Validators;
using Trampline.Contracts.DTOs.Requests;
using Trampline.Core.Models;

namespace Trampline.Tests.Validators;

public class UpdateStatusValidatorTests
{
    private readonly UpdateStatusValidator _validator = new();

    [Theory]
    [InlineData(ApplicationStatus.Pending)]
    [InlineData(ApplicationStatus.Viewed)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Invited)]
    [InlineData(ApplicationStatus.Hired)]
    [InlineData(ApplicationStatus.Withdrawn)]
    public void Valid_Statuses_PassValidation(ApplicationStatus status)
    {
        var model = new UpdateStatusRequest { Status = status };
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Invalid_Status_Fails()
    {
        var model = new UpdateStatusRequest { Status = (ApplicationStatus)999 };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }
}
