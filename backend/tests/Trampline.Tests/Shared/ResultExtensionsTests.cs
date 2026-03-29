using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Trampline.Shared.Results;
using Trampline.Web.Extensions;

namespace Trampline.Tests.Shared;

public class ResultExtensionsTests
{
    #region Non-generic ToActionResult

    [Fact]
    public void ToActionResult_Success_ReturnsOk()
    {
        var result = Result.Success();

        var actionResult = result.ToActionResult();

        actionResult.Should().BeOfType<OkResult>();
    }

    [Fact]
    public void ToActionResult_Failure401_ReturnsUnauthorized()
    {
        var result = Result.Failure(new ErrorDetail("token", "Invalid", 401));

        var actionResult = result.ToActionResult();

        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public void ToActionResult_Failure403_ReturnsForbidden()
    {
        var result = Result.Failure(new ErrorDetail("user", "Access denied", 403));

        var actionResult = result.ToActionResult();

        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(403);
    }

    [Fact]
    public void ToActionResult_Failure404_ReturnsNotFound()
    {
        var result = Result.Failure(new ErrorDetail("resource", "Not found", 404));

        var actionResult = result.ToActionResult();

        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public void ToActionResult_FailureDefault_ReturnsBadRequest()
    {
        var result = Result.Failure(new ErrorDetail("field", "Validation error"));

        var actionResult = result.ToActionResult();

        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(400);
    }

    #endregion

    #region Generic ToActionResult<T>

    [Fact]
    public void GenericToActionResult_Success_ReturnsOkWithValue()
    {
        var result = Result<string>.Success("data");

        var actionResult = result.ToActionResult();

        var okResult = actionResult as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be("data");
    }

    [Fact]
    public void GenericToActionResult_Failure401_ReturnsUnauthorized()
    {
        var result = Result<string>.Failure(new ErrorDetail("auth", "Invalid token", 401));

        var actionResult = result.ToActionResult();

        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public void GenericToActionResult_Failure404_ReturnsNotFound()
    {
        var result = Result<string>.Failure(new ErrorDetail("id", "Not found", 404));

        var actionResult = result.ToActionResult();

        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public void GenericToActionResult_FailureDefault_ReturnsBadRequest()
    {
        var result = Result<string>.Failure(new ErrorDetail("field", "Error"));

        var actionResult = result.ToActionResult();

        var objectResult = actionResult as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(400);
    }

    [Fact]
    public void GenericToActionResult_ReturnsProblemJson()
    {
        var result = Result<string>.Failure(new ErrorDetail("f", "m", 400));

        var actionResult = result.ToActionResult();

        var objectResult = actionResult as ObjectResult;
        objectResult!.ContentTypes.Should().Contain("application/problem+json");
    }

    #endregion
}
