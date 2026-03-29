using FluentAssertions;
using Trampline.Shared.Results;

namespace Trampline.Tests.Shared;

public class ResultTests
{
    #region Result (non-generic)

    [Fact]
    public void Success_IsSuccess_ReturnsTrue()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithSingleError_IsFailure()
    {
        var error = new ErrorDetail("field", "error message", 400);

        var result = Result.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Field.Should().Be("field");
        result.Errors[0].Message.Should().Be("error message");
        result.Errors[0].Code.Should().Be(400);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ContainsAllErrors()
    {
        var errors = new[]
        {
            new ErrorDetail("field1", "error1"),
            new ErrorDetail("field2", "error2"),
            new ErrorDetail("field3", "error3", 404),
        };

        var result = Result.Failure(errors);

        result.Errors.Should().HaveCount(3);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ErrorCode_TakesFromFirstError()
    {
        var result = Result.Failure(
            new ErrorDetail("f1", "m1", 401),
            new ErrorDetail("f2", "m2", 403));

        result.ErrorCode.Should().Be(401);
    }

    [Fact]
    public void ErrorCode_WhenNoCode_IsNull()
    {
        var result = Result.Failure(new ErrorDetail("f", "m"));

        result.ErrorCode.Should().BeNull();
    }

    #endregion

    #region Result<T> (generic)

    [Fact]
    public void GenericSuccess_ContainsValue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void GenericSuccess_WithStringValue()
    {
        var result = Result<string>.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void GenericFailure_ValueIsDefault()
    {
        var result = Result<int>.Failure(new ErrorDetail("f", "m"));

        result.IsFailure.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public void GenericFailure_WithReferenceType_ValueIsNull()
    {
        var result = Result<string>.Failure(new ErrorDetail("f", "m"));

        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void GenericFailure_WithMultipleErrors()
    {
        var result = Result<string>.Failure(
            new ErrorDetail("f1", "m1", 400),
            new ErrorDetail("f2", "m2", 404));

        result.Errors.Should().HaveCount(2);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ToResult_FromGenericSuccess_ReturnsSuccess()
    {
        var generic = Result<int>.Success(42);

        var result = generic.ToResult();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToResult_FromGenericFailure_ReturnsFailureWithErrors()
    {
        var generic = Result<int>.Failure(new ErrorDetail("f", "m", 400));

        var result = generic.ToResult();

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
    }

    #endregion

    #region ErrorDetail

    [Fact]
    public void ErrorDetail_Properties()
    {
        var error = new ErrorDetail("email", "Invalid format", 422);

        error.Field.Should().Be("email");
        error.Message.Should().Be("Invalid format");
        error.Code.Should().Be(422);
    }

    [Fact]
    public void ErrorDetail_DefaultCode_IsNull()
    {
        var error = new ErrorDetail("field", "message");

        error.Code.Should().BeNull();
    }

    [Fact]
    public void ErrorDetail_Equality_ByValue()
    {
        var error1 = new ErrorDetail("f", "m", 400);
        var error2 = new ErrorDetail("f", "m", 400);

        error1.Should().Be(error2);
    }

    #endregion
}
